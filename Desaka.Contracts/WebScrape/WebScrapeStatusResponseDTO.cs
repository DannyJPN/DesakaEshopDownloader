using Desaka.Contracts.Common;

namespace Desaka.Contracts.WebScrape;

public sealed record WebScrapeStatusResponseDTO(long RunId, JobStatus Status, int? Progress, DateTime StartedAt, DateTime? UpdatedAt, string? Message);
