using Desaka.Config.Application;
using Desaka.Contracts.Common;
using Desaka.Contracts.Config;
using Desaka.DataAccess;
using Desaka.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desaka.Config.Infrastructure;

public sealed class EfConfigService : IConfigService
{
    private readonly DesakaDbContext _db;
    private readonly ISecretProtector _protector;

    public EfConfigService(DesakaDbContext db, ISecretProtector protector)
    {
        _db = db;
        _protector = protector;
    }

    public async Task<IReadOnlyList<EshopConfigDto>> GetEshopsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.ConfigEshops.AsNoTracking().ToListAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<EshopConfigDto> UpsertEshopAsync(EshopConfigDto eshop, CancellationToken cancellationToken = default)
    {
        ConfigEshop entity;
        if (eshop.Id == 0)
        {
            entity = new ConfigEshop();
            _db.ConfigEshops.Add(entity);
        }
        else
        {
            entity = await _db.ConfigEshops.FirstOrDefaultAsync(x => x.Id == eshop.Id, cancellationToken)
                ?? new ConfigEshop();
            if (entity.Id == 0)
            {
                _db.ConfigEshops.Add(entity);
            }
        }

        entity.Name = eshop.Name;
        entity.BaseUrl = eshop.BaseUrl;
        entity.IsEnabled = eshop.IsEnabled;
        entity.DownloadSchedule = eshop.DownloadSchedule;
        entity.PriceListSource = eshop.PriceListSource;
        entity.LanguageFlags = eshop.LanguageFlags;

        await _db.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<bool> DeleteEshopAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ConfigEshops.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _db.ConfigEshops.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<AutopollRuleConfigDto>> GetAutopollRulesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.ConfigAutopollRules.AsNoTracking().ToListAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<AutopollRuleConfigDto> UpsertAutopollRuleAsync(AutopollRuleConfigDto rule, CancellationToken cancellationToken = default)
    {
        ConfigAutopollRule entity;
        if (rule.Id == 0)
        {
            entity = new ConfigAutopollRule();
            _db.ConfigAutopollRules.Add(entity);
        }
        else
        {
            entity = await _db.ConfigAutopollRules.FirstOrDefaultAsync(x => x.Id == rule.Id, cancellationToken)
                ?? new ConfigAutopollRule();
            if (entity.Id == 0)
            {
                _db.ConfigAutopollRules.Add(entity);
            }
        }

        entity.Name = rule.Name;
        entity.IsEnabled = rule.IsEnabled;
        entity.EshopId = rule.EshopId;
        entity.IntervalValue = rule.IntervalValue;
        entity.IntervalUnit = rule.IntervalUnit.ToString().ToLowerInvariant();
        entity.WindowStart = rule.WindowStart;
        entity.WindowEnd = rule.WindowEnd;
        entity.FilterDefinition = rule.FilterDefinition;

        await _db.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<bool> DeleteAutopollRuleAsync(int id, CancellationToken cancellationToken = default)
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

    public async Task<PathsConfigDto> GetPathsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _db.ConfigSettings.AsNoTracking().ToListAsync(cancellationToken);
        return new PathsConfigDto(
            GetSetting(settings, "paths.output_root"),
            GetSetting(settings, "paths.raw_root"),
            GetSetting(settings, "paths.images_root"),
            GetSetting(settings, "paths.reports_root"),
            GetSetting(settings, "paths.logs_root"),
            GetSetting(settings, "paths.autopoll_reports_root"),
            GetSetting(settings, "paths.temp_root"));
    }

    public async Task<PathsConfigDto> UpdatePathsAsync(PathsConfigDto paths, CancellationToken cancellationToken = default)
    {
        await UpsertSettingAsync("paths.output_root", paths.OutputRoot, cancellationToken);
        await UpsertSettingAsync("paths.raw_root", paths.RawRoot, cancellationToken);
        await UpsertSettingAsync("paths.images_root", paths.ImagesRoot, cancellationToken);
        await UpsertSettingAsync("paths.reports_root", paths.ReportsRoot, cancellationToken);
        await UpsertSettingAsync("paths.logs_root", paths.LogsRoot, cancellationToken);
        await UpsertSettingAsync("paths.autopoll_reports_root", paths.AutopollReportsRoot, cancellationToken);
        await UpsertSettingAsync("paths.temp_root", paths.TempRoot, cancellationToken);
        return paths;
    }

    public async Task<IReadOnlyList<LanguageConfigDto>> GetLanguagesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.ConfigLanguages.AsNoTracking().ToListAsync(cancellationToken);
        return items.Select(x => new LanguageConfigDto(x.Code, x.Name ?? string.Empty, x.IsEnabled, x.IsDefault)).ToList();
    }

    public async Task<LanguageConfigDto> UpsertLanguageAsync(LanguageConfigDto language, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ConfigLanguages.FirstOrDefaultAsync(x => x.Code == language.Code, cancellationToken)
            ?? new ConfigLanguage();

        entity.Code = language.Code;
        entity.Name = language.Name;
        entity.IsEnabled = language.IsEnabled;
        entity.IsDefault = language.IsDefault;

        if (entity.Id == 0)
        {
            _db.ConfigLanguages.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return language;
    }

    public async Task<IReadOnlyList<AiConfigDto>> GetAiConfigsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.ConfigAiProviderAssignments.AsNoTracking().ToListAsync(cancellationToken);
        return items.Select(x => new AiConfigDto(x.TaskName, x.ProviderId, x.ModelName, _protector.Unprotect(x.ApiKeyEncrypted), x.IsEnabled)).ToList();
    }

    public async Task<AiConfigDto> UpsertAiConfigAsync(AiConfigDto config, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ConfigAiProviderAssignments.FirstOrDefaultAsync(x => x.TaskName == config.TaskName, cancellationToken)
            ?? new ConfigAiProviderAssignment();

        entity.TaskName = config.TaskName;
        entity.ProviderId = config.ProviderId;
        entity.ModelName = config.ModelName;
        entity.ApiKeyEncrypted = _protector.Protect(config.ApiKeyPlaintext);
        entity.IsEnabled = config.IsEnabled;

        if (entity.Id == 0)
        {
            _db.ConfigAiProviderAssignments.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return config;
    }

    private static EshopConfigDto Map(ConfigEshop entity)
        => new(entity.Id, entity.Name, entity.BaseUrl, entity.IsEnabled, entity.DownloadSchedule, entity.PriceListSource, entity.LanguageFlags);

    private static AutopollRuleConfigDto Map(ConfigAutopollRule entity)
    {
        var intervalUnit = Enum.TryParse<IntervalUnit>(entity.IntervalUnit, true, out var parsed)
            ? parsed
            : IntervalUnit.Hour;

        return new AutopollRuleConfigDto(entity.Id, entity.Name, entity.IsEnabled, entity.EshopId, entity.IntervalValue, intervalUnit, entity.WindowStart, entity.WindowEnd, entity.FilterDefinition);
    }

    private static string GetSetting(IEnumerable<ConfigSetting> settings, string key)
        => settings.FirstOrDefault(x => x.Key == key)?.Value ?? string.Empty;

    private async Task UpsertSettingAsync(string key, string value, CancellationToken cancellationToken)
    {
        var setting = await _db.ConfigSettings.FirstOrDefaultAsync(x => x.Key == key, cancellationToken)
            ?? new ConfigSetting { Key = key, Category = "paths" };

        setting.Value = value;
        if (setting.Id == 0)
        {
            _db.ConfigSettings.Add(setting);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
