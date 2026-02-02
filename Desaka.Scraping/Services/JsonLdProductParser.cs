using System.Text.Json;
using System.Text.RegularExpressions;
using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services;

public static class JsonLdProductParser
{
    private static readonly Regex JsonLdRegex = new("<script[^>]*type=[\"']application/ld\\+json[\"'][^>]*>(.*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    public static ScrapedProduct? TryParse(byte[] htmlBytes)
    {
        var html = System.Text.Encoding.UTF8.GetString(htmlBytes);
        var matches = JsonLdRegex.Matches(html);
        foreach (Match match in matches)
        {
            var json = match.Groups[1].Value.Trim();
            if (string.IsNullOrWhiteSpace(json))
            {
                continue;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var product = ExtractProduct(doc.RootElement);
                if (product != null && !string.IsNullOrWhiteSpace(product.Name))
                {
                    return product;
                }
            }
            catch
            {
                continue;
            }
        }

        return null;
    }

    private static ScrapedProduct? ExtractProduct(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var result = ExtractProduct(item);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (element.TryGetProperty("@graph", out var graph) && graph.ValueKind == JsonValueKind.Array)
        {
            foreach (var node in graph.EnumerateArray())
            {
                var result = ExtractProduct(node);
                if (result != null)
                {
                    return result;
                }
            }
        }

        if (element.TryGetProperty("@type", out var typeValue))
        {
            if (typeValue.ValueKind == JsonValueKind.String && !string.Equals(typeValue.GetString(), "Product", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (typeValue.ValueKind == JsonValueKind.Array)
            {
                var hasProduct = typeValue.EnumerateArray().Any(x => string.Equals(x.GetString(), "Product", StringComparison.OrdinalIgnoreCase));
                if (!hasProduct)
                {
                    return null;
                }
            }
        }

        var product = new ScrapedProduct
        {
            Name = ReadString(element, "name") ?? string.Empty,
            Description = ReadString(element, "description"),
            ShortDescription = ReadString(element, "shortDescription"),
            Url = ReadString(element, "url") ?? string.Empty
        };

        if (element.TryGetProperty("image", out var imageValue))
        {
            if (imageValue.ValueKind == JsonValueKind.String)
            {
                product.MainPhotoUrl = imageValue.GetString();
            }
            else if (imageValue.ValueKind == JsonValueKind.Array)
            {
                foreach (var img in imageValue.EnumerateArray())
                {
                    var imgUrl = img.GetString();
                    if (string.IsNullOrWhiteSpace(imgUrl))
                    {
                        continue;
                    }

                    if (product.MainPhotoUrl == null)
                    {
                        product.MainPhotoUrl = imgUrl;
                    }
                    else
                    {
                        product.GalleryUrls.Add(imgUrl);
                    }
                }
            }
        }

        if (element.TryGetProperty("offers", out var offers))
        {
            ApplyOffers(product, offers);
        }

        return product;
    }

    private static void ApplyOffers(ScrapedProduct product, JsonElement offers)
    {
        if (offers.ValueKind == JsonValueKind.Array)
        {
            foreach (var offer in offers.EnumerateArray())
            {
                ApplyOffer(product, offer);
            }
            return;
        }

        if (offers.ValueKind == JsonValueKind.Object)
        {
            ApplyOffer(product, offers);
        }
    }

    private static void ApplyOffer(ScrapedProduct product, JsonElement offer)
    {
        if (offer.TryGetProperty("price", out var priceValue))
        {
            if (priceValue.ValueKind == JsonValueKind.Number && priceValue.TryGetDecimal(out var price))
            {
                product.Variants.Add(new ScrapedVariant { CurrentPrice = price });
                return;
            }

            if (priceValue.ValueKind == JsonValueKind.String && decimal.TryParse(priceValue.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            {
                product.Variants.Add(new ScrapedVariant { CurrentPrice = parsed });
            }
        }
    }

    private static string? ReadString(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }
}
