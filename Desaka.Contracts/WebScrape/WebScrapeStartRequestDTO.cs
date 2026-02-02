using Desaka.Contracts.Common;

namespace Desaka.Contracts.WebScrape;

public sealed record WebScrapeStartRequestDTO(int EshopId, bool Force = false, string? CorrelationId = null);
