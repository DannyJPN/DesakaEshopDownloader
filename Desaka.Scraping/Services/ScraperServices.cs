using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services;

public interface IExtractor
{
    Task<ScrapedProduct> ExtractAsync(ScrapeContent content, CancellationToken cancellationToken = default);
}

public interface IScraper
{
    Task<ScrapeResult> ScrapeAsync(ScrapeRequest request, CancellationToken cancellationToken = default);
}

public sealed class HttpScraper : IScraper
{
    private readonly HttpClient _httpClient;
    private readonly IExtractor _extractor;

    public HttpScraper(HttpClient httpClient, IExtractor extractor)
    {
        _httpClient = httpClient;
        _extractor = extractor;
    }

    public async Task<ScrapeResult> ScrapeAsync(ScrapeRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, request.Url);
        if (!string.IsNullOrWhiteSpace(request.UserAgent))
        {
            message.Headers.UserAgent.ParseAdd(request.UserAgent);
        }

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var hash = ComputeHash(body);

        var content = new ScrapeContent
        {
            Url = request.Url,
            ContentType = request.ContentType,
            Body = body,
            Hash = hash,
            DownloadedAt = DateTime.UtcNow
        };

        var product = await _extractor.ExtractAsync(content, cancellationToken);
        return new ScrapeResult { Content = content, Product = product };
    }

    private static string ComputeHash(ReadOnlySpan<byte> bytes)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
