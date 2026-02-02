using System.Text.Json;
using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services;

public static class PincesobchodCatalogParser
{
    public static IReadOnlyList<ScrapedProduct> Parse(byte[] jsonBytes, string? languageCode)
    {
        using var doc = JsonDocument.Parse(jsonBytes);
        if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ScrapedProduct>();
        }

        var lang = (languageCode ?? "cs").ToLowerInvariant();
        var result = new List<ScrapedProduct>();

        foreach (var productEl in data.EnumerateArray())
        {
            var product = new ScrapedProduct
            {
                Url = productEl.TryGetProperty("url", out var url) ? url.GetString() ?? string.Empty : string.Empty,
                Name = ReadTranslation(productEl, lang, "name"),
                Description = ReadTranslation(productEl, lang, "description"),
                ShortDescription = ReadTranslation(productEl, lang, "annotation")
            };

            if (productEl.TryGetProperty("images", out var images) && images.ValueKind == JsonValueKind.Array)
            {
                foreach (var image in images.EnumerateArray())
                {
                    if (image.TryGetProperty("url", out var imageUrl))
                    {
                        var urlValue = imageUrl.GetString();
                        if (!string.IsNullOrWhiteSpace(urlValue))
                        {
                            if (image.TryGetProperty("default", out var def) && def.ValueKind == JsonValueKind.True)
                            {
                                product.MainPhotoUrl = urlValue;
                            }
                            else
                            {
                                product.GalleryUrls.Add(urlValue);
                            }
                        }
                    }
                }
            }

            result.Add(product);
        }

        return result;
    }

    private static string? ReadTranslation(JsonElement productEl, string lang, string property)
    {
        if (!productEl.TryGetProperty("translations", out var translations))
        {
            return null;
        }

        if (translations.TryGetProperty(lang, out var langBlock) && langBlock.ValueKind == JsonValueKind.Object)
        {
            if (langBlock.TryGetProperty(property, out var value))
            {
                return value.GetString();
            }
        }

        return null;
    }
}
