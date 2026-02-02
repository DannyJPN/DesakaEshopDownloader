using System.Xml.Linq;
using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services;

public sealed class GenericSitemapScraper : ISiteScraper
{
    private readonly HttpClient _httpClient;
    private readonly EshopScraperRegistry _registry;

    public GenericSitemapScraper(HttpClient httpClient, EshopScraperRegistry registry)
    {
        _httpClient = httpClient;
        _registry = registry;
    }

    public async Task<IReadOnlyList<ScrapeResult>> ScrapeAsync(SiteScrapeRequest request, CancellationToken cancellationToken = default)
    {
        var definition = _registry.Resolve(request.BaseUrl);
        if (definition == null)
        {
            return Array.Empty<ScrapeResult>();
        }

        var sitemapUrls = await LoadSitemapUrlsAsync(definition.SitemapUrl, cancellationToken);
        var results = new List<ScrapeResult>();

        foreach (var url in sitemapUrls)
        {
            if (request.MaxProducts.HasValue && results.Count >= request.MaxProducts.Value)
            {
                break;
            }

            var result = await TryDownloadProductAsync(url, request.UserAgent, cancellationToken);
            if (result != null)
            {
                results.Add(result);
            }
        }

        return results;
    }

    private async Task<IReadOnlyList<string>> LoadSitemapUrlsAsync(string sitemapUrl, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(sitemapUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<string>();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = XDocument.Parse(content);
        var urls = new List<string>();

        if (doc.Root == null)
        {
            return urls;
        }

        var rootName = doc.Root.Name.LocalName;
        if (string.Equals(rootName, "sitemapindex", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var sitemap in doc.Descendants().Where(x => x.Name.LocalName == "loc"))
            {
                var childUrl = sitemap.Value.Trim();
                if (!string.IsNullOrWhiteSpace(childUrl))
                {
                    urls.AddRange(await LoadSitemapUrlsAsync(childUrl, cancellationToken));
                }
            }

            return urls;
        }

        foreach (var loc in doc.Descendants().Where(x => x.Name.LocalName == "loc"))
        {
            var url = loc.Value.Trim();
            if (!string.IsNullOrWhiteSpace(url))
            {
                urls.Add(url);
            }
        }

        return urls;
    }

    private async Task<ScrapeResult?> TryDownloadProductAsync(string url, string? userAgent, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            request.Headers.UserAgent.ParseAdd(userAgent);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var body = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(body)).ToLowerInvariant();
        var product = JsonLdProductParser.TryParse(body);
        if (product == null)
        {
            return null;
        }

        return new ScrapeResult
        {
            Content = new ScrapeContent
            {
                Url = url,
                ContentType = ScrapeContentType.Html,
                Body = body,
                Hash = hash,
                DownloadedAt = DateTime.UtcNow
            },
            Product = product
        };
    }
}
