using Desaka.Contracts.WebScrape;

namespace Desaka.WebScrape.Application;

public interface IWebScrapeService
{
    Task<WebScrapeStartResponseDTO> StartAsync(WebScrapeStartRequestDTO request, CancellationToken cancellationToken = default);
    Task<WebScrapeStatusResponseDTO?> GetStatusAsync(long? runId, int? eshopId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WebScrapeStatusResponseDTO>> ListAsync(string? status, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}

