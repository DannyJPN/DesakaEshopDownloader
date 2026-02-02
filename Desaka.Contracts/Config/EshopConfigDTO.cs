using Desaka.Contracts.Common;

namespace Desaka.Contracts.Config;

public sealed record EshopConfigDTO(int Id, string Name, string BaseUrl, bool IsEnabled, string DownloadSchedule, string PriceListSource, string? LanguageFlags);
