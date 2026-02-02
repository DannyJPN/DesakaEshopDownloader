using Desaka.Contracts.Common;

namespace Desaka.Contracts.WebScrape;

public sealed record WebScrapeStartResponseDTO(long RunId, JobStatus Status);
