using Desaka.Contracts.Autopoll;
using Desaka.Contracts.Common;
using Desaka.DataAccess;
using Desaka.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desaka.Autopolling.Infrastructure;

public sealed class AutopollBatchCommitService
{
    private readonly DesakaDbContext _db;
    private readonly Desaka.Export.ExportService _exportService;

    public AutopollBatchCommitService(DesakaDbContext db, Desaka.Export.ExportService exportService)
    {
        _db = db;
        _exportService = exportService;
    }

    public async Task<AutopollBatchCommitResponseDTO> CommitAsync(AutopollBatchCommitRequestDTO request, CancellationToken cancellationToken = default)
    {
        var batch = new AutopollBatchCommit
        {
            StartAt = DateTime.UtcNow,
            Status = "running"
        };

        _db.AutopollBatchCommits.Add(batch);
        await _db.SaveChangesAsync(cancellationToken);

        var snapshots = _db.AutopollSnapshots.AsQueryable();
        if (request.From.HasValue)
        {
            snapshots = snapshots.Where(x => x.CapturedAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            snapshots = snapshots.Where(x => x.CapturedAt <= request.To.Value);
        }

        var toApply = await snapshots.Take(2000).ToListAsync(cancellationToken);
        foreach (var snapshot in toApply)
        {
            var existing = await _db.ProductsCurrent.FirstOrDefaultAsync(x => x.Url == snapshot.Url, cancellationToken);
            if (existing == null)
            {
                var mapped = MapSnapshot(snapshot);
                mapped.CreatedAt = DateTime.UtcNow;
                mapped.UpdatedAt = DateTime.UtcNow;
                _db.ProductsCurrent.Add(mapped);
            }
            else
            {
                ApplySnapshot(existing, snapshot);
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        var exportRoot = await ResolvePathAsync("paths.autopoll_reports_root", "data/autopoll", cancellationToken);
        var mapped = toApply.Select(MapSnapshot).ToList();

        var export = await _exportService.ExportProductsAsync(mapped, new Desaka.Export.ExportOptions
        {
            OutputDirectory = exportRoot,
            Format = Desaka.Export.ExportFormat.Csv,
            FileNamePrefix = "Autopoll_Report"
        }, cancellationToken);

        batch.ItemsApplied = toApply.Count;
        batch.Status = "success";
        batch.EndAt = DateTime.UtcNow;
        batch.ReportPath = export.RelativePath;

        await _db.SaveChangesAsync(cancellationToken);
        return new AutopollBatchCommitResponseDTO(batch.Id, JobStatus.Success);
    }

    private async Task<string> ResolvePathAsync(string key, string fallback, CancellationToken cancellationToken)
    {
        var setting = await _db.ConfigSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        return string.IsNullOrWhiteSpace(setting?.Value) ? fallback : setting.Value;
    }

    private static ProductsCurrent MapSnapshot(AutopollSnapshot snapshot)
    {
        return new ProductsCurrent
        {
            Kod = snapshot.Kod,
            Url = snapshot.Url,
            UrlDomain = snapshot.UrlDomain,
            IdOutput = snapshot.IdOutput,
            Typ = snapshot.Typ,
            VariantaId = snapshot.VariantaId,
            Varianta1Nazev = snapshot.Varianta1Nazev,
            Varianta1Hodnota = snapshot.Varianta1Hodnota,
            Varianta2Nazev = snapshot.Varianta2Nazev,
            Varianta2Hodnota = snapshot.Varianta2Hodnota,
            Varianta3Nazev = snapshot.Varianta3Nazev,
            Varianta3Hodnota = snapshot.Varianta3Hodnota,
            VariantaStejne = snapshot.VariantaStejne,
            Zobrazit = snapshot.Zobrazit,
            Archiv = snapshot.Archiv,
            KodVyrobku = snapshot.KodVyrobku,
            Ean = snapshot.Ean,
            Isbn = snapshot.Isbn,
            Nazev = snapshot.Nazev,
            Privlastek = snapshot.Privlastek,
            Vyrobce = snapshot.Vyrobce,
            DodavatelId = snapshot.DodavatelId,
            Cena = snapshot.Cena,
            CenaBezna = snapshot.CenaBezna,
            CenaNakupni = snapshot.CenaNakupni,
            RecyklacniPoplatek = snapshot.RecyklacniPoplatek,
            Dph = snapshot.Dph,
            Sleva = snapshot.Sleva,
            SlevaOd = snapshot.SlevaOd,
            SlevaDo = snapshot.SlevaDo,
            Popis = snapshot.Popis,
            PopisStrucny = snapshot.PopisStrucny,
            Kosik = snapshot.Kosik,
            Home = snapshot.Home,
            Dostupnost = snapshot.Dostupnost,
            DopravaZdarma = snapshot.DopravaZdarma,
            DodaciDoba = snapshot.DodaciDoba,
            DodaciDobaAuto = snapshot.DodaciDobaAuto,
            Sklad = snapshot.Sklad,
            NaSklade = snapshot.NaSklade,
            Hmotnost = snapshot.Hmotnost,
            Delka = snapshot.Delka,
            Jednotka = snapshot.Jednotka,
            OdberPo = snapshot.OdberPo,
            OdberMin = snapshot.OdberMin,
            OdberMax = snapshot.OdberMax,
            Pocet = snapshot.Pocet,
            Zaruka = snapshot.Zaruka,
            MarzeDodavatel = snapshot.MarzeDodavatel,
            SeoTitulek = snapshot.SeoTitulek,
            SeoPopis = snapshot.SeoPopis,
            Eroticke = snapshot.Eroticke,
            ProDospele = snapshot.ProDospele,
            SlevovyKupon = snapshot.SlevovyKupon,
            DarekObjednavka = snapshot.DarekObjednavka,
            Priorita = snapshot.Priorita,
            Poznamka = snapshot.Poznamka,
            DodavatelKod = snapshot.DodavatelKod,
            Stitky = snapshot.Stitky,
            CenaDodavatel = snapshot.CenaDodavatel,
            KategorieId = snapshot.KategorieId,
            Podobne = snapshot.Podobne,
            Prislusenstvi = snapshot.Prislusenstvi,
            Variantove = snapshot.Variantove,
            Zdarma = snapshot.Zdarma,
            Sluzby = snapshot.Sluzby,
            RozsirujiciObsah = snapshot.RozsirujiciObsah,
            ZboziCzSkryt = snapshot.ZboziCzSkryt,
            ZboziCzProductname = snapshot.ZboziCzProductname,
            ZboziCzProduct = snapshot.ZboziCzProduct,
            ZboziCzCpc = snapshot.ZboziCzCpc,
            ZboziCzCpcSearch = snapshot.ZboziCzCpcSearch,
            ZboziCzKategorie = snapshot.ZboziCzKategorie,
            ZboziCzStitek0 = snapshot.ZboziCzStitek0,
            ZboziCzStitek1 = snapshot.ZboziCzStitek1,
            ZboziCzExtra = snapshot.ZboziCzExtra,
            HeurekaCzSkryt = snapshot.HeurekaCzSkryt,
            HeurekaCzProductname = snapshot.HeurekaCzProductname,
            HeurekaCzProduct = snapshot.HeurekaCzProduct,
            HeurekaCzCpc = snapshot.HeurekaCzCpc,
            HeurekaCzKategorie = snapshot.HeurekaCzKategorie,
            GoogleSkryt = snapshot.GoogleSkryt,
            GoogleKategorie = snapshot.GoogleKategorie,
            GoogleStitek0 = snapshot.GoogleStitek0,
            GoogleStitek1 = snapshot.GoogleStitek1,
            GoogleStitek2 = snapshot.GoogleStitek2,
            GoogleStitek3 = snapshot.GoogleStitek3,
            GoogleStitek4 = snapshot.GoogleStitek4,
            GlamiSkryt = snapshot.GlamiSkryt,
            GlamiKategorie = snapshot.GlamiKategorie,
            GlamiCpc = snapshot.GlamiCpc,
            GlamiVoucher = snapshot.GlamiVoucher,
            GlamiMaterial = snapshot.GlamiMaterial,
            GlamiSkMaterial = snapshot.GlamiSkMaterial,
            SkladUmisteni = snapshot.SkladUmisteni,
            SkladMinimalni = snapshot.SkladMinimalni,
            SkladOptimalni = snapshot.SkladOptimalni,
            SkladMaximalni = snapshot.SkladMaximalni
        };
    }

    private static void ApplySnapshot(ProductsCurrent target, AutopollSnapshot snapshot)
    {
        target.Kod = snapshot.Kod;
        target.Url = snapshot.Url;
        target.UrlDomain = snapshot.UrlDomain;
        target.IdOutput = snapshot.IdOutput;
        target.Typ = snapshot.Typ;
        target.VariantaId = snapshot.VariantaId;
        target.Varianta1Nazev = snapshot.Varianta1Nazev;
        target.Varianta1Hodnota = snapshot.Varianta1Hodnota;
        target.Varianta2Nazev = snapshot.Varianta2Nazev;
        target.Varianta2Hodnota = snapshot.Varianta2Hodnota;
        target.Varianta3Nazev = snapshot.Varianta3Nazev;
        target.Varianta3Hodnota = snapshot.Varianta3Hodnota;
        target.VariantaStejne = snapshot.VariantaStejne;
        target.Zobrazit = snapshot.Zobrazit;
        target.Archiv = snapshot.Archiv;
        target.KodVyrobku = snapshot.KodVyrobku;
        target.Ean = snapshot.Ean;
        target.Isbn = snapshot.Isbn;
        target.Nazev = snapshot.Nazev;
        target.Privlastek = snapshot.Privlastek;
        target.Vyrobce = snapshot.Vyrobce;
        target.DodavatelId = snapshot.DodavatelId;
        target.Cena = snapshot.Cena;
        target.CenaBezna = snapshot.CenaBezna;
        target.CenaNakupni = snapshot.CenaNakupni;
        target.RecyklacniPoplatek = snapshot.RecyklacniPoplatek;
        target.Dph = snapshot.Dph;
        target.Sleva = snapshot.Sleva;
        target.SlevaOd = snapshot.SlevaOd;
        target.SlevaDo = snapshot.SlevaDo;
        target.Popis = snapshot.Popis;
        target.PopisStrucny = snapshot.PopisStrucny;
        target.Kosik = snapshot.Kosik;
        target.Home = snapshot.Home;
        target.Dostupnost = snapshot.Dostupnost;
        target.DopravaZdarma = snapshot.DopravaZdarma;
        target.DodaciDoba = snapshot.DodaciDoba;
        target.DodaciDobaAuto = snapshot.DodaciDobaAuto;
        target.Sklad = snapshot.Sklad;
        target.NaSklade = snapshot.NaSklade;
        target.Hmotnost = snapshot.Hmotnost;
        target.Delka = snapshot.Delka;
        target.Jednotka = snapshot.Jednotka;
        target.OdberPo = snapshot.OdberPo;
        target.OdberMin = snapshot.OdberMin;
        target.OdberMax = snapshot.OdberMax;
        target.Pocet = snapshot.Pocet;
        target.Zaruka = snapshot.Zaruka;
        target.MarzeDodavatel = snapshot.MarzeDodavatel;
        target.SeoTitulek = snapshot.SeoTitulek;
        target.SeoPopis = snapshot.SeoPopis;
        target.Eroticke = snapshot.Eroticke;
        target.ProDospele = snapshot.ProDospele;
        target.SlevovyKupon = snapshot.SlevovyKupon;
        target.DarekObjednavka = snapshot.DarekObjednavka;
        target.Priorita = snapshot.Priorita;
        target.Poznamka = snapshot.Poznamka;
        target.DodavatelKod = snapshot.DodavatelKod;
        target.Stitky = snapshot.Stitky;
        target.CenaDodavatel = snapshot.CenaDodavatel;
        target.KategorieId = snapshot.KategorieId;
        target.Podobne = snapshot.Podobne;
        target.Prislusenstvi = snapshot.Prislusenstvi;
        target.Variantove = snapshot.Variantove;
        target.Zdarma = snapshot.Zdarma;
        target.Sluzby = snapshot.Sluzby;
        target.RozsirujiciObsah = snapshot.RozsirujiciObsah;
        target.ZboziCzSkryt = snapshot.ZboziCzSkryt;
        target.ZboziCzProductname = snapshot.ZboziCzProductname;
        target.ZboziCzProduct = snapshot.ZboziCzProduct;
        target.ZboziCzCpc = snapshot.ZboziCzCpc;
        target.ZboziCzCpcSearch = snapshot.ZboziCzCpcSearch;
        target.ZboziCzKategorie = snapshot.ZboziCzKategorie;
        target.ZboziCzStitek0 = snapshot.ZboziCzStitek0;
        target.ZboziCzStitek1 = snapshot.ZboziCzStitek1;
        target.ZboziCzExtra = snapshot.ZboziCzExtra;
        target.HeurekaCzSkryt = snapshot.HeurekaCzSkryt;
        target.HeurekaCzProductname = snapshot.HeurekaCzProductname;
        target.HeurekaCzProduct = snapshot.HeurekaCzProduct;
        target.HeurekaCzCpc = snapshot.HeurekaCzCpc;
        target.HeurekaCzKategorie = snapshot.HeurekaCzKategorie;
        target.GoogleSkryt = snapshot.GoogleSkryt;
        target.GoogleKategorie = snapshot.GoogleKategorie;
        target.GoogleStitek0 = snapshot.GoogleStitek0;
        target.GoogleStitek1 = snapshot.GoogleStitek1;
        target.GoogleStitek2 = snapshot.GoogleStitek2;
        target.GoogleStitek3 = snapshot.GoogleStitek3;
        target.GoogleStitek4 = snapshot.GoogleStitek4;
        target.GlamiSkryt = snapshot.GlamiSkryt;
        target.GlamiKategorie = snapshot.GlamiKategorie;
        target.GlamiCpc = snapshot.GlamiCpc;
        target.GlamiVoucher = snapshot.GlamiVoucher;
        target.GlamiMaterial = snapshot.GlamiMaterial;
        target.GlamiSkMaterial = snapshot.GlamiSkMaterial;
        target.SkladUmisteni = snapshot.SkladUmisteni;
        target.SkladMinimalni = snapshot.SkladMinimalni;
        target.SkladOptimalni = snapshot.SkladOptimalni;
        target.SkladMaximalni = snapshot.SkladMaximalni;
    }
}

