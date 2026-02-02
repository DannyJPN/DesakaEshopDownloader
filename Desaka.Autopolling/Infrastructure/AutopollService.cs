using Desaka.Autopolling.Application;
using Desaka.Contracts.Autopoll;
using Desaka.Contracts.Common;
using Desaka.Contracts.EventBus;
using Desaka.Comparation;
using Desaka.DataAccess;
using Desaka.DataAccess.Entities;
using Desaka.EventBus;
using Microsoft.EntityFrameworkCore;

namespace Desaka.Autopolling.Infrastructure;

public sealed class AutopollService : IAutopollService
{
    private readonly DesakaDbContext _db;
    private readonly IEventBus _eventBus;
    private readonly IRuntimeLockService _lockService;
    private readonly IProductComparator _comparator;
    private readonly AutopollFilterEngine _filterEngine;

    public AutopollService(
        DesakaDbContext db,
        IEventBus eventBus,
        IRuntimeLockService lockService,
        IProductComparator comparator,
        AutopollFilterEngine filterEngine)
    {
        _db = db;
        _eventBus = eventBus;
        _lockService = lockService;
        _comparator = comparator;
        _filterEngine = filterEngine;
    }

    public async Task<AutopollStartResponseDTO> StartAsync(AutopollStartRequestDTO request, CancellationToken cancellationToken = default)
    {
        var existing = await _db.AutopollRuns.AsNoTracking()
            .Where(x => request.RuleId.HasValue && x.RuleId == request.RuleId.Value && x.Status == "running")
            .OrderByDescending(x => x.StartAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != null)
        {
            return new AutopollStartResponseDTO(existing.Id, JobStatus.Running);
        }

        var run = new AutopollRun
        {
            RuleId = request.RuleId,
            StartAt = DateTime.UtcNow,
            Status = "running"
        };

        _db.AutopollRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        if (request.EshopId.HasValue)
        {
            var webscrapeLock = $"webscrape:eshop:{request.EshopId.Value}";
            if (await _lockService.IsLockedAsync(webscrapeLock, cancellationToken))
            {
                run.Status = "cancelled";
                run.ErrorMessage = "WebScrape running for same eshop.";
                run.EndAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
                return new AutopollStartResponseDTO(run.Id, JobStatus.Cancelled);
            }
        }

        var lockKey = request.RuleId.HasValue ? $"autopoll:rule:{request.RuleId.Value}" : $"autopoll:eshop:{request.EshopId}";
        var ownerId = $"autopoll:{run.Id}";
        var acquired = await _lockService.TryAcquireAsync(lockKey, ownerId, TimeSpan.FromHours(1), cancellationToken);
        if (!acquired)
        {
            run.Status = "cancelled";
            run.ErrorMessage = "Runtime lock busy.";
            run.EndAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return new AutopollStartResponseDTO(run.Id, JobStatus.Cancelled);
        }

        await _eventBus.PublishAsync(new EventEnvelope(
            "AutopollRunStarted",
            "Autopolling",
            DateTime.UtcNow,
            request.CorrelationId,
            System.Text.Json.JsonSerializer.Serialize(new { run.Id, run.RuleId })),
            cancellationToken);

        await CaptureSnapshotAsync(run, request.EshopId, cancellationToken);

        await _lockService.ReleaseAsync(lockKey, ownerId, cancellationToken);

        return new AutopollStartResponseDTO(run.Id, JobStatus.Running);
    }

    public async Task<AutopollStatusResponseDTO?> GetStatusAsync(long? runId, int? ruleId, CancellationToken cancellationToken = default)
    {
        var query = _db.AutopollRuns.AsNoTracking().AsQueryable();
        if (runId.HasValue)
        {
            query = query.Where(x => x.Id == runId.Value);
        }
        else if (ruleId.HasValue)
        {
            query = query.Where(x => x.RuleId == ruleId.Value)
                .OrderByDescending(x => x.StartAt)
                .Take(1);
        }
        else
        {
            return null;
        }

        var run = await query.FirstOrDefaultAsync(cancellationToken);
        if (run == null)
        {
            return null;
        }

        return new AutopollStatusResponseDTO(run.Id, ParseStatus(run.Status), null, run.StartAt, run.EndAt, run.ErrorMessage);
    }

    public async Task<IReadOnlyList<AutopollRuleDTO>> GetRulesAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _db.ConfigAutopollRules.AsNoTracking().ToListAsync(cancellationToken);
        return rules.Select(Map).ToList();
    }

    public async Task<AutopollRuleDTO> UpsertRuleAsync(AutopollRuleDTO rule, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ConfigAutopollRules.FirstOrDefaultAsync(x => x.Id == rule.Id, cancellationToken)
            ?? new ConfigAutopollRule();

        entity.Name = rule.Name;
        entity.IsEnabled = rule.IsEnabled;
        entity.EshopId = rule.EshopId;
        entity.IntervalValue = rule.IntervalValue;
        entity.IntervalUnit = rule.IntervalUnit.ToString().ToLowerInvariant();
        entity.WindowStart = rule.WindowStart;
        entity.WindowEnd = rule.WindowEnd;
        entity.FilterDefinition = rule.FilterDefinition;
        entity.LastRunAt = rule.LastRunAt;
        entity.NextRunAt = rule.NextRunAt;
        entity.Note = rule.Note;

        if (entity.Id == 0)
        {
            _db.ConfigAutopollRules.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<bool> DeleteRuleAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ConfigAutopollRules.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _db.ConfigAutopollRules.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static AutopollRuleDTO Map(ConfigAutopollRule entity)
    {
        var intervalUnit = Enum.TryParse<IntervalUnit>(entity.IntervalUnit, true, out var parsed)
            ? parsed
            : IntervalUnit.Hour;

        return new AutopollRuleDTO(entity.Id, entity.Name, entity.IsEnabled, entity.EshopId, entity.IntervalValue, intervalUnit, entity.WindowStart, entity.WindowEnd, entity.FilterDefinition, entity.LastRunAt, entity.NextRunAt, entity.Note);
    }

    private static JobStatus ParseStatus(string status)
        => status switch
        {
            "running" => JobStatus.Running,
            "success" => JobStatus.Success,
            "error" => JobStatus.Error,
            "cancelled" => JobStatus.Cancelled,
            _ => JobStatus.Queued
        };

    private async Task CaptureSnapshotAsync(AutopollRun run, int? eshopId, CancellationToken cancellationToken)
    {
        var productsQuery = _db.ProductsCurrent.AsNoTracking().AsQueryable();
        if (eshopId.HasValue)
        {
            var eshop = await _db.ConfigEshops.AsNoTracking().FirstOrDefaultAsync(x => x.Id == eshopId.Value, cancellationToken);
            if (eshop != null)
            {
                var domain = GetDomain(eshop.BaseUrl);
                productsQuery = productsQuery.Where(x => x.UrlDomain == domain);
            }
        }

        var products = await productsQuery.ToListAsync(cancellationToken);
        var filterDefinition = await ResolveFilterDefinitionAsync(run.RuleId, cancellationToken);
        var itemFilter = await _db.MemoryItemFilter.AsNoTracking().ToListAsync(cancellationToken);
        var blockedWrongs = await _db.MemoryWrongs.AsNoTracking().ToListAsync(cancellationToken);

        var filtered = products.Where(p =>
            _filterEngine.Matches(p, filterDefinition)
            && !IsBlockedByItemFilter(p, itemFilter)
            && !IsBlockedByWrongs(p, blockedWrongs)).ToList();

        var checkedCount = 0;
        var changedCount = 0;

        foreach (var product in filtered)
        {
            checkedCount++;
            var latest = await _db.AutopollSnapshots.AsNoTracking()
                .Where(x => x.Url == product.Url)
                .OrderByDescending(x => x.CapturedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var snapshot = CreateSnapshot(product, run.Id);
            if (latest != null && latest.ChangeHash == snapshot.ChangeHash)
            {
                continue;
            }

            _db.AutopollSnapshots.Add(snapshot);
            await _db.SaveChangesAsync(cancellationToken);
            if (latest != null)
            {
                var comparison = _comparator.Compare(product, latest);
                if (!comparison.IsMatch)
                {
                    _db.AutopollChangeLogs.Add(new AutopollChangeLog
                    {
                        SnapshotId = snapshot.Id,
                        ChangedFields = string.Join(',', comparison.Changes.Select(x => x.Field)),
                        OldValues = string.Join("|", comparison.Changes.Select(x => $"{x.Field}:{x.OldValue}")),
                        NewValues = string.Join("|", comparison.Changes.Select(x => $"{x.Field}:{x.NewValue}"))
                    });
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }

            changedCount++;
        }

        run.ItemsChecked = checkedCount;
        run.ItemsChanged = changedCount;
        run.Status = "success";
        run.EndAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<string?> ResolveFilterDefinitionAsync(long? ruleId, CancellationToken cancellationToken)
    {
        if (!ruleId.HasValue)
        {
            return null;
        }

        var rule = await _db.ConfigAutopollRules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ruleId.Value, cancellationToken);
        return rule?.FilterDefinition;
    }

    private static bool IsBlockedByItemFilter(ProductsCurrent product, IReadOnlyList<MemoryItemFilter> entries)
    {
        var name = product.Nazev ?? string.Empty;
        var code = product.Kod ?? string.Empty;
        foreach (var entry in entries)
        {
            if (!string.IsNullOrWhiteSpace(entry.UrlDomain) &&
                !string.Equals(entry.UrlDomain, product.UrlDomain, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.Key))
            {
                continue;
            }

            if (name.Contains(entry.Key, StringComparison.OrdinalIgnoreCase) ||
                code.Contains(entry.Key, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsBlockedByWrongs(ProductsCurrent product, IReadOnlyList<MemoryWrongs> entries)
    {
        var name = product.Nazev ?? string.Empty;
        var code = product.Kod ?? string.Empty;
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
            {
                continue;
            }

            if (name.Contains(entry.Key, StringComparison.OrdinalIgnoreCase) ||
                code.Contains(entry.Key, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static AutopollSnapshot CreateSnapshot(ProductsCurrent product, long runId)
    {
        var hashSource = $"{product.Kod}|{product.Nazev}|{product.Cena}|{product.CenaBezna}|{product.PopisStrucny}|{product.Popis}";
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(hashSource))).ToLowerInvariant();

        return new AutopollSnapshot
        {
            Url = product.Url,
            UrlDomain = product.UrlDomain,
            CapturedAt = DateTime.UtcNow,
            SourceRunId = runId,
            ChangeHash = hash,
            Kod = product.Kod,
            IdOutput = product.IdOutput,
            Typ = product.Typ,
            VariantaId = product.VariantaId,
            Varianta1Nazev = product.Varianta1Nazev,
            Varianta1Hodnota = product.Varianta1Hodnota,
            Varianta2Nazev = product.Varianta2Nazev,
            Varianta2Hodnota = product.Varianta2Hodnota,
            Varianta3Nazev = product.Varianta3Nazev,
            Varianta3Hodnota = product.Varianta3Hodnota,
            VariantaStejne = product.VariantaStejne,
            Zobrazit = product.Zobrazit,
            Archiv = product.Archiv,
            KodVyrobku = product.KodVyrobku,
            Ean = product.Ean,
            Isbn = product.Isbn,
            Nazev = product.Nazev,
            Privlastek = product.Privlastek,
            Vyrobce = product.Vyrobce,
            DodavatelId = product.DodavatelId,
            Cena = product.Cena,
            CenaBezna = product.CenaBezna,
            CenaNakupni = product.CenaNakupni,
            RecyklacniPoplatek = product.RecyklacniPoplatek,
            Dph = product.Dph,
            Sleva = product.Sleva,
            SlevaOd = product.SlevaOd,
            SlevaDo = product.SlevaDo,
            Popis = product.Popis,
            PopisStrucny = product.PopisStrucny,
            Kosik = product.Kosik,
            Home = product.Home,
            Dostupnost = product.Dostupnost,
            DopravaZdarma = product.DopravaZdarma,
            DodaciDoba = product.DodaciDoba,
            DodaciDobaAuto = product.DodaciDobaAuto,
            Sklad = product.Sklad,
            NaSklade = product.NaSklade,
            Hmotnost = product.Hmotnost,
            Delka = product.Delka,
            Jednotka = product.Jednotka,
            OdberPo = product.OdberPo,
            OdberMin = product.OdberMin,
            OdberMax = product.OdberMax,
            Pocet = product.Pocet,
            Zaruka = product.Zaruka,
            MarzeDodavatel = product.MarzeDodavatel,
            SeoTitulek = product.SeoTitulek,
            SeoPopis = product.SeoPopis,
            Eroticke = product.Eroticke,
            ProDospele = product.ProDospele,
            SlevovyKupon = product.SlevovyKupon,
            DarekObjednavka = product.DarekObjednavka,
            Priorita = product.Priorita,
            Poznamka = product.Poznamka,
            DodavatelKod = product.DodavatelKod,
            Stitky = product.Stitky,
            CenaDodavatel = product.CenaDodavatel,
            KategorieId = product.KategorieId,
            Podobne = product.Podobne,
            Prislusenstvi = product.Prislusenstvi,
            Variantove = product.Variantove,
            Zdarma = product.Zdarma,
            Sluzby = product.Sluzby,
            RozsirujiciObsah = product.RozsirujiciObsah,
            ZboziCzSkryt = product.ZboziCzSkryt,
            ZboziCzProductname = product.ZboziCzProductname,
            ZboziCzProduct = product.ZboziCzProduct,
            ZboziCzCpc = product.ZboziCzCpc,
            ZboziCzCpcSearch = product.ZboziCzCpcSearch,
            ZboziCzKategorie = product.ZboziCzKategorie,
            ZboziCzStitek0 = product.ZboziCzStitek0,
            ZboziCzStitek1 = product.ZboziCzStitek1,
            ZboziCzExtra = product.ZboziCzExtra,
            HeurekaCzSkryt = product.HeurekaCzSkryt,
            HeurekaCzProductname = product.HeurekaCzProductname,
            HeurekaCzProduct = product.HeurekaCzProduct,
            HeurekaCzCpc = product.HeurekaCzCpc,
            HeurekaCzKategorie = product.HeurekaCzKategorie,
            GoogleSkryt = product.GoogleSkryt,
            GoogleKategorie = product.GoogleKategorie,
            GoogleStitek0 = product.GoogleStitek0,
            GoogleStitek1 = product.GoogleStitek1,
            GoogleStitek2 = product.GoogleStitek2,
            GoogleStitek3 = product.GoogleStitek3,
            GoogleStitek4 = product.GoogleStitek4,
            GlamiSkryt = product.GlamiSkryt,
            GlamiKategorie = product.GlamiKategorie,
            GlamiCpc = product.GlamiCpc,
            GlamiVoucher = product.GlamiVoucher,
            GlamiMaterial = product.GlamiMaterial,
            GlamiSkMaterial = product.GlamiSkMaterial,
            SkladUmisteni = product.SkladUmisteni,
            SkladMinimalni = product.SkladMinimalni,
            SkladOptimalni = product.SkladOptimalni,
            SkladMaximalni = product.SkladMaximalni
        };
    }

    private static string GetDomain(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri.Host.ToLowerInvariant();
        }

        return url;
    }
}

