using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services;

public sealed class NullExtractor : IExtractor
{
    public Task<ScrapedProduct> ExtractAsync(ScrapeContent content, CancellationToken cancellationToken = default)
    {
        var product = new ScrapedProduct
        {
            Url = content.Url,
            Name = "",
            Description = null,
            ShortDescription = null
        };

        return Task.FromResult(product);
    }
}
