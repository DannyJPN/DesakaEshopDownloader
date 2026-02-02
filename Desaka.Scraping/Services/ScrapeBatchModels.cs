using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services;

public sealed class SiteScrapeRequest
{
    public string BaseUrl { get; set; } = "";
    public string? UserAgent { get; set; }
    public int? MaxProducts { get; set; }
}

public interface ISiteScraper
{
    Task<IReadOnlyList<ScrapeResult>> ScrapeAsync(SiteScrapeRequest request, CancellationToken cancellationToken = default);
}
