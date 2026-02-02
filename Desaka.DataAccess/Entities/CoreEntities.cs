namespace Desaka.DataAccess.Entities;

public sealed class ProductsCurrent
{
    public long Id { get; set; }
    public string Kod { get; set; } = "";
    public string Url { get; set; } = "";
    public string UrlDomain { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string? IdOutput { get; set; }
    public string? Typ { get; set; }
    public string? VariantaId { get; set; }
    public string? Varianta1Nazev { get; set; }
    public string? Varianta1Hodnota { get; set; }
    public string? Varianta2Nazev { get; set; }
    public string? Varianta2Hodnota { get; set; }
    public string? Varianta3Nazev { get; set; }
    public string? Varianta3Hodnota { get; set; }
    public string? VariantaStejne { get; set; }
    public bool? Zobrazit { get; set; }
    public bool? Archiv { get; set; }
    public string? KodVyrobku { get; set; }
    public string? Ean { get; set; }
    public string? Isbn { get; set; }
    public string? Nazev { get; set; }
    public string? Privlastek { get; set; }
    public string? Vyrobce { get; set; }
    public string? DodavatelId { get; set; }
    public decimal? Cena { get; set; }
    public decimal? CenaBezna { get; set; }
    public decimal? CenaNakupni { get; set; }
    public decimal? RecyklacniPoplatek { get; set; }
    public decimal? Dph { get; set; }
    public decimal? Sleva { get; set; }
    public DateTime? SlevaOd { get; set; }
    public DateTime? SlevaDo { get; set; }
    public string? Popis { get; set; }
    public string? PopisStrucny { get; set; }
    public bool? Kosik { get; set; }
    public bool? Home { get; set; }
    public int? Dostupnost { get; set; }
    public bool? DopravaZdarma { get; set; }
    public int? DodaciDoba { get; set; }
    public bool? DodaciDobaAuto { get; set; }
    public int? Sklad { get; set; }
    public int? NaSklade { get; set; }
    public decimal? Hmotnost { get; set; }
    public decimal? Delka { get; set; }
    public string? Jednotka { get; set; }
    public int? OdberPo { get; set; }
    public int? OdberMin { get; set; }
    public int? OdberMax { get; set; }
    public int? Pocet { get; set; }
    public int? Zaruka { get; set; }
    public decimal? MarzeDodavatel { get; set; }
    public string? SeoTitulek { get; set; }
    public string? SeoPopis { get; set; }
    public bool? Eroticke { get; set; }
    public bool? ProDospele { get; set; }
    public bool? SlevovyKupon { get; set; }
    public bool? DarekObjednavka { get; set; }
    public int? Priorita { get; set; }
    public string? Poznamka { get; set; }
    public string? DodavatelKod { get; set; }
    public string? Stitky { get; set; }
    public decimal? CenaDodavatel { get; set; }
    public string? KategorieId { get; set; }
    public string? Podobne { get; set; }
    public string? Prislusenstvi { get; set; }
    public string? Variantove { get; set; }
    public bool? Zdarma { get; set; }
    public string? Sluzby { get; set; }
    public string? RozsirujiciObsah { get; set; }
    public bool? ZboziCzSkryt { get; set; }
    public string? ZboziCzProductname { get; set; }
    public string? ZboziCzProduct { get; set; }
    public decimal? ZboziCzCpc { get; set; }
    public decimal? ZboziCzCpcSearch { get; set; }
    public string? ZboziCzKategorie { get; set; }
    public string? ZboziCzStitek0 { get; set; }
    public string? ZboziCzStitek1 { get; set; }
    public string? ZboziCzExtra { get; set; }
    public bool? HeurekaCzSkryt { get; set; }
    public string? HeurekaCzProductname { get; set; }
    public string? HeurekaCzProduct { get; set; }
    public decimal? HeurekaCzCpc { get; set; }
    public string? HeurekaCzKategorie { get; set; }
    public bool? GoogleSkryt { get; set; }
    public string? GoogleKategorie { get; set; }
    public string? GoogleStitek0 { get; set; }
    public string? GoogleStitek1 { get; set; }
    public string? GoogleStitek2 { get; set; }
    public string? GoogleStitek3 { get; set; }
    public string? GoogleStitek4 { get; set; }
    public bool? GlamiSkryt { get; set; }
    public string? GlamiKategorie { get; set; }
    public decimal? GlamiCpc { get; set; }
    public bool? GlamiVoucher { get; set; }
    public string? GlamiMaterial { get; set; }
    public string? GlamiSkMaterial { get; set; }
    public string? SkladUmisteni { get; set; }
    public int? SkladMinimalni { get; set; }
    public int? SkladOptimalni { get; set; }
    public int? SkladMaximalni { get; set; }
}
public sealed class ProductsHistory
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string ChangeSource { get; set; } = "";
    public string Kod { get; set; } = "";
    public string Url { get; set; } = "";
    public string UrlDomain { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? IdOutput { get; set; }
    public string? Typ { get; set; }
    public string? VariantaId { get; set; }
    public string? Varianta1Nazev { get; set; }
    public string? Varianta1Hodnota { get; set; }
    public string? Varianta2Nazev { get; set; }
    public string? Varianta2Hodnota { get; set; }
    public string? Varianta3Nazev { get; set; }
    public string? Varianta3Hodnota { get; set; }
    public string? VariantaStejne { get; set; }
    public bool? Zobrazit { get; set; }
    public bool? Archiv { get; set; }
    public string? KodVyrobku { get; set; }
    public string? Ean { get; set; }
    public string? Isbn { get; set; }
    public string? Nazev { get; set; }
    public string? Privlastek { get; set; }
    public string? Vyrobce { get; set; }
    public string? DodavatelId { get; set; }
    public decimal? Cena { get; set; }
    public decimal? CenaBezna { get; set; }
    public decimal? CenaNakupni { get; set; }
    public decimal? RecyklacniPoplatek { get; set; }
    public decimal? Dph { get; set; }
    public decimal? Sleva { get; set; }
    public DateTime? SlevaOd { get; set; }
    public DateTime? SlevaDo { get; set; }
    public string? Popis { get; set; }
    public string? PopisStrucny { get; set; }
    public bool? Kosik { get; set; }
    public bool? Home { get; set; }
    public int? Dostupnost { get; set; }
    public bool? DopravaZdarma { get; set; }
    public int? DodaciDoba { get; set; }
    public bool? DodaciDobaAuto { get; set; }
    public int? Sklad { get; set; }
    public int? NaSklade { get; set; }
    public decimal? Hmotnost { get; set; }
    public decimal? Delka { get; set; }
    public string? Jednotka { get; set; }
    public int? OdberPo { get; set; }
    public int? OdberMin { get; set; }
    public int? OdberMax { get; set; }
    public int? Pocet { get; set; }
    public int? Zaruka { get; set; }
    public decimal? MarzeDodavatel { get; set; }
    public string? SeoTitulek { get; set; }
    public string? SeoPopis { get; set; }
    public bool? Eroticke { get; set; }
    public bool? ProDospele { get; set; }
    public bool? SlevovyKupon { get; set; }
    public bool? DarekObjednavka { get; set; }
    public int? Priorita { get; set; }
    public string? Poznamka { get; set; }
    public string? DodavatelKod { get; set; }
    public string? Stitky { get; set; }
    public decimal? CenaDodavatel { get; set; }
    public string? KategorieId { get; set; }
    public string? Podobne { get; set; }
    public string? Prislusenstvi { get; set; }
    public string? Variantove { get; set; }
    public bool? Zdarma { get; set; }
    public string? Sluzby { get; set; }
    public string? RozsirujiciObsah { get; set; }
    public bool? ZboziCzSkryt { get; set; }
    public string? ZboziCzProductname { get; set; }
    public string? ZboziCzProduct { get; set; }
    public decimal? ZboziCzCpc { get; set; }
    public decimal? ZboziCzCpcSearch { get; set; }
    public string? ZboziCzKategorie { get; set; }
    public string? ZboziCzStitek0 { get; set; }
    public string? ZboziCzStitek1 { get; set; }
    public string? ZboziCzExtra { get; set; }
    public bool? HeurekaCzSkryt { get; set; }
    public string? HeurekaCzProductname { get; set; }
    public string? HeurekaCzProduct { get; set; }
    public decimal? HeurekaCzCpc { get; set; }
    public string? HeurekaCzKategorie { get; set; }
    public bool? GoogleSkryt { get; set; }
    public string? GoogleKategorie { get; set; }
    public string? GoogleStitek0 { get; set; }
    public string? GoogleStitek1 { get; set; }
    public string? GoogleStitek2 { get; set; }
    public string? GoogleStitek3 { get; set; }
    public string? GoogleStitek4 { get; set; }
    public bool? GlamiSkryt { get; set; }
    public string? GlamiKategorie { get; set; }
    public decimal? GlamiCpc { get; set; }
    public bool? GlamiVoucher { get; set; }
    public string? GlamiMaterial { get; set; }
    public string? GlamiSkMaterial { get; set; }
    public string? SkladUmisteni { get; set; }
    public int? SkladMinimalni { get; set; }
    public int? SkladOptimalni { get; set; }
    public int? SkladMaximalni { get; set; }
}

public sealed class AutopollSnapshot
{
    public long Id { get; set; }
    public DateTime CapturedAt { get; set; }
    public long SourceRunId { get; set; }
    public string ChangeHash { get; set; } = "";
    public string Kod { get; set; } = "";
    public string Url { get; set; } = "";
    public string UrlDomain { get; set; } = "";
    public string? IdOutput { get; set; }
    public string? Typ { get; set; }
    public string? VariantaId { get; set; }
    public string? Varianta1Nazev { get; set; }
    public string? Varianta1Hodnota { get; set; }
    public string? Varianta2Nazev { get; set; }
    public string? Varianta2Hodnota { get; set; }
    public string? Varianta3Nazev { get; set; }
    public string? Varianta3Hodnota { get; set; }
    public string? VariantaStejne { get; set; }
    public bool? Zobrazit { get; set; }
    public bool? Archiv { get; set; }
    public string? KodVyrobku { get; set; }
    public string? Ean { get; set; }
    public string? Isbn { get; set; }
    public string? Nazev { get; set; }
    public string? Privlastek { get; set; }
    public string? Vyrobce { get; set; }
    public string? DodavatelId { get; set; }
    public decimal? Cena { get; set; }
    public decimal? CenaBezna { get; set; }
    public decimal? CenaNakupni { get; set; }
    public decimal? RecyklacniPoplatek { get; set; }
    public decimal? Dph { get; set; }
    public decimal? Sleva { get; set; }
    public DateTime? SlevaOd { get; set; }
    public DateTime? SlevaDo { get; set; }
    public string? Popis { get; set; }
    public string? PopisStrucny { get; set; }
    public bool? Kosik { get; set; }
    public bool? Home { get; set; }
    public int? Dostupnost { get; set; }
    public bool? DopravaZdarma { get; set; }
    public int? DodaciDoba { get; set; }
    public bool? DodaciDobaAuto { get; set; }
    public int? Sklad { get; set; }
    public int? NaSklade { get; set; }
    public decimal? Hmotnost { get; set; }
    public decimal? Delka { get; set; }
    public string? Jednotka { get; set; }
    public int? OdberPo { get; set; }
    public int? OdberMin { get; set; }
    public int? OdberMax { get; set; }
    public int? Pocet { get; set; }
    public int? Zaruka { get; set; }
    public decimal? MarzeDodavatel { get; set; }
    public string? SeoTitulek { get; set; }
    public string? SeoPopis { get; set; }
    public bool? Eroticke { get; set; }
    public bool? ProDospele { get; set; }
    public bool? SlevovyKupon { get; set; }
    public bool? DarekObjednavka { get; set; }
    public int? Priorita { get; set; }
    public string? Poznamka { get; set; }
    public string? DodavatelKod { get; set; }
    public string? Stitky { get; set; }
    public decimal? CenaDodavatel { get; set; }
    public string? KategorieId { get; set; }
    public string? Podobne { get; set; }
    public string? Prislusenstvi { get; set; }
    public string? Variantove { get; set; }
    public bool? Zdarma { get; set; }
    public string? Sluzby { get; set; }
    public string? RozsirujiciObsah { get; set; }
    public bool? ZboziCzSkryt { get; set; }
    public string? ZboziCzProductname { get; set; }
    public string? ZboziCzProduct { get; set; }
    public decimal? ZboziCzCpc { get; set; }
    public decimal? ZboziCzCpcSearch { get; set; }
    public string? ZboziCzKategorie { get; set; }
    public string? ZboziCzStitek0 { get; set; }
    public string? ZboziCzStitek1 { get; set; }
    public string? ZboziCzExtra { get; set; }
    public bool? HeurekaCzSkryt { get; set; }
    public string? HeurekaCzProductname { get; set; }
    public string? HeurekaCzProduct { get; set; }
    public decimal? HeurekaCzCpc { get; set; }
    public string? HeurekaCzKategorie { get; set; }
    public bool? GoogleSkryt { get; set; }
    public string? GoogleKategorie { get; set; }
    public string? GoogleStitek0 { get; set; }
    public string? GoogleStitek1 { get; set; }
    public string? GoogleStitek2 { get; set; }
    public string? GoogleStitek3 { get; set; }
    public string? GoogleStitek4 { get; set; }
    public bool? GlamiSkryt { get; set; }
    public string? GlamiKategorie { get; set; }
    public decimal? GlamiCpc { get; set; }
    public bool? GlamiVoucher { get; set; }
    public string? GlamiMaterial { get; set; }
    public string? GlamiSkMaterial { get; set; }
    public string? SkladUmisteni { get; set; }
    public int? SkladMinimalni { get; set; }
    public int? SkladOptimalni { get; set; }
    public int? SkladMaximalni { get; set; }
}



public sealed class ProductVariant
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string Variantcode { get; set; } = "";
    public decimal? CurrentPrice { get; set; }
    public decimal? BasicPrice { get; set; }
    public string? StockStatus { get; set; }
}

public sealed class VariantOption
{
    public long Id { get; set; }
    public long VariantId { get; set; }
    public string OptionName { get; set; } = "";
    public string OptionValue { get; set; } = "";
}

public sealed class ScrapeRun
{
    public long Id { get; set; }
    public int EshopId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string Status { get; set; } = "";
    public string? ErrorMessage { get; set; }
    public int? ItemsTotal { get; set; }
    public int? ItemsChanged { get; set; }
    public int? ItemsSkipped { get; set; }
    public long? DurationMs { get; set; }
}

public sealed class ScrapeRawMetadata
{
    public long Id { get; set; }
    public int EshopId { get; set; }
    public string Url { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string ContentHash { get; set; } = "";
    public long? ContentSize { get; set; }
    public string ContentType { get; set; } = "";
    public DateTime DownloadedAt { get; set; }
}

public sealed class DownloadedProduct
{
    public long Id { get; set; }
    public int EshopId { get; set; }
    public string Name { get; set; } = "";
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? MainPhotoPath { get; set; }
    public string Url { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public sealed class DownloadedGallery
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string Filepath { get; set; } = "";
}

public sealed class DownloadedVariant
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public decimal? CurrentPrice { get; set; }
    public decimal? BasicPrice { get; set; }
    public string? StockStatus { get; set; }
}

public sealed class DownloadedVariantOption
{
    public long Id { get; set; }
    public long VariantId { get; set; }
    public string OptionName { get; set; } = "";
    public string OptionValue { get; set; } = "";
}

public sealed class AutopollRun
{
    public long Id { get; set; }
    public long? RuleId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string Status { get; set; } = "";
    public string? ErrorMessage { get; set; }
    public int? ItemsChecked { get; set; }
    public int? ItemsChanged { get; set; }
    public long? DurationMs { get; set; }
}


public sealed class AutopollChangeLog
{
    public long Id { get; set; }
    public long SnapshotId { get; set; }
    public string? ChangedFields { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}

public sealed class AutopollBatchCommit
{
    public long Id { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string Status { get; set; } = "";
    public int? ItemsApplied { get; set; }
    public string? ReportPath { get; set; }
    public bool NotifySent { get; set; }
}

public sealed class UnifierRun
{
    public long Id { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string Status { get; set; } = "";
    public string? ErrorMessage { get; set; }
    public int? ItemsTotal { get; set; }
    public int? ItemsProcessed { get; set; }
}

public sealed class UnifierApproval
{
    public long Id { get; set; }
    public string ProductCode { get; set; } = "";
    public string Url { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string PropertyName { get; set; } = "";
    public string? CurrentValue { get; set; }
    public string? SuggestedValue { get; set; }
    public string Language { get; set; } = "";
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public sealed class ConfigEshop
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public bool IsEnabled { get; set; }
    public string DownloadSchedule { get; set; } = "";
    public string PriceListSource { get; set; } = "";
    public string? LanguageFlags { get; set; }
}

public sealed class ConfigAutopollRule
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsEnabled { get; set; }
    public int EshopId { get; set; }
    public int IntervalValue { get; set; }
    public string IntervalUnit { get; set; } = "";
    public TimeSpan? WindowStart { get; set; }
    public TimeSpan? WindowEnd { get; set; }
    public string FilterDefinition { get; set; } = "";
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public string? Note { get; set; }
}

public sealed class ConfigSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string? Value { get; set; }
    public string? Category { get; set; }
}

public sealed class ConfigLanguage
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string? Name { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }
}

public sealed class ConfigAiProviderCatalog
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool? SupportsBatch { get; set; }
    public bool? SupportsImages { get; set; }
    public bool? SupportsText { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ConfigAiProviderAssignment
{
    public int Id { get; set; }
    public string TaskName { get; set; } = "";
    public int ProviderId { get; set; }
    public string ModelName { get; set; } = "";
    public string ApiKeyEncrypted { get; set; } = "";
    public bool IsEnabled { get; set; }
}

public sealed class ConfigAiPricing
{
    public int Id { get; set; }
    public int ProviderId { get; set; }
    public string ModelName { get; set; } = "";
    public decimal PriceInputPer1K { get; set; }
    public decimal PriceOutputPer1K { get; set; }
    public string Currency { get; set; } = "USD";
    public string? SourceUrl { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
}

public sealed class ConfigPincesobchodApiKey
{
    public int Id { get; set; }
    public string LanguageCode { get; set; } = "";
    public string ApiKeyEncrypted { get; set; } = "";
    public bool IsEnabled { get; set; }
}

public sealed class AuditLog
{
    public long Id { get; set; }
    public DateTime EventAt { get; set; }
    public string ActorType { get; set; } = "";
    public string? ActorId { get; set; }
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string? EntityId { get; set; }
    public string? Summary { get; set; }
    public string? DetailsJson { get; set; }
    public string? SourceService { get; set; }
    public string? CorrelationId { get; set; }
}

public sealed class AiUsageLog
{
    public long Id { get; set; }
    public DateTime EventAt { get; set; }
    public int ProviderId { get; set; }
    public string ModelName { get; set; } = "";
    public string TaskName { get; set; } = "";
    public string? RequestId { get; set; }
    public int? TokensIn { get; set; }
    public int? TokensOut { get; set; }
    public decimal CostUsd { get; set; }
    public string Currency { get; set; } = "USD";
    public string PriceSource { get; set; } = "";
    public string? CorrelationId { get; set; }
}

public sealed class RuntimeLock
{
    public long Id { get; set; }
    public string ResourceKey { get; set; } = "";
    public string OwnerId { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Status { get; set; } = "";
}

public sealed class RuntimeState
{
    public long Id { get; set; }
    public string ServiceName { get; set; } = "";
    public int? EshopId { get; set; }
    public string State { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string? LastMessage { get; set; }
}

public sealed class NotifyDeviceToken
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = "";
    public string Platform { get; set; } = "";
    public string Token { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
