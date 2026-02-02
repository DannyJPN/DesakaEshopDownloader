using System.Text.Json;
using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services;

public sealed class PincesobchodJsonExtractor : IExtractor
{
    public Task<ScrapedProduct> ExtractAsync(ScrapeContent content, CancellationToken cancellationToken = default)
    {
        var doc = JsonDocument.Parse(content.Body);
        var root = doc.RootElement;
        var product = new ScrapedProduct
        {
            Url = root.TryGetProperty("url", out var url) ? url.GetString() ?? string.Empty : string.Empty,
            Name = root.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
            Description = root.TryGetProperty("description", out var desc) ? desc.GetString() : null,
            ShortDescription = root.TryGetProperty("annotation", out var shortDesc) ? shortDesc.GetString() : null
        };

        if (root.TryGetProperty("photos", out var photos) && photos.ValueKind == JsonValueKind.Array)
        {
            foreach (var photo in photos.EnumerateArray())
            {
                if (photo.TryGetProperty("url", out var photoUrl))
                {
                    var value = photoUrl.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        product.GalleryUrls.Add(value);
                    }
                }
            }
        }

        return Task.FromResult(product);
    }
}
