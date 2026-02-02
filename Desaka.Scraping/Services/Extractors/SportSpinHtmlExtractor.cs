using System.Text;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services.Extractors;

/// <summary>
/// HTML extractor for sportspin.cz e-shop.
/// Parses product details, variants, prices, stock status, and images.
/// </summary>
public sealed partial class SportSpinHtmlExtractor : IExtractor
{
    private static readonly Regex PriceRegex = GeneratePriceRegex();

    public async Task<ScrapedProduct> ExtractAsync(ScrapeContent content, CancellationToken cancellationToken = default)
    {
        var html = Encoding.UTF8.GetString(content.Body);
        var config = Configuration.Default;
        using var context = BrowsingContext.New(config);
        using var document = await context.OpenAsync(req => req.Content(html), cancellationToken);

        var product = new ScrapedProduct
        {
            Url = ExtractCanonicalUrl(document) ?? content.Url,
            Name = ExtractName(document),
            ShortDescription = ExtractShortDescription(document),
            Description = ExtractDescription(document),
            MainPhotoUrl = ExtractMainPhoto(document)
        };

        // Extract gallery images
        var galleryUrls = ExtractGallery(document);
        foreach (var url in galleryUrls)
        {
            product.GalleryUrls.Add(url);
        }

        // Extract variants
        var variants = ExtractVariants(document);
        if (variants.Count == 0)
        {
            // Create a single default variant with fallback price/stock
            var fallbackVariant = CreateFallbackVariant(document);
            product.Variants.Add(fallbackVariant);
        }
        else
        {
            foreach (var variant in variants)
            {
                product.Variants.Add(variant);
            }
        }

        return product;
    }

    private static string ExtractName(IDocument document)
    {
        var element = document.QuerySelector("div.p-detail-inner-header > h1");
        return element?.TextContent.Trim() ?? string.Empty;
    }

    private static string? ExtractShortDescription(IDocument document)
    {
        var element = document.QuerySelector("div.p-short-description[data-testid='productCardShortDescr']");
        return element?.TextContent.Trim();
    }

    private static string? ExtractDescription(IDocument document)
    {
        var element = document.QuerySelector("div.basic-description");
        return element?.TextContent.Trim();
    }

    private static string? ExtractCanonicalUrl(IDocument document)
    {
        var element = document.QuerySelector("meta[property='og:url']");
        return element?.GetAttribute("content");
    }

    private static string? ExtractMainPhoto(IDocument document)
    {
        var element = document.QuerySelector("a.p-main-image.cloud-zoom");
        return element?.GetAttribute("data-href");
    }

    private static List<string> ExtractGallery(IDocument document)
    {
        var urls = new List<string>();
        var elements = document.QuerySelectorAll("a.p-thumbnail[href]");

        foreach (var element in elements)
        {
            var href = element.GetAttribute("href");
            if (!string.IsNullOrWhiteSpace(href))
            {
                urls.Add(href);
            }
        }

        return urls;
    }

    private static List<ScrapedVariant> ExtractVariants(IDocument document)
    {
        var variants = new List<ScrapedVariant>();
        var variantRows = document.QuerySelectorAll("div.table-row[data-testid='productVariant']");

        foreach (var row in variantRows)
        {
            var variant = new ScrapedVariant
            {
                CurrentPrice = ExtractVariantCurrentPrice(row),
                BasicPrice = ExtractVariantBasicPrice(row),
                StockStatus = ExtractVariantStock(row)
            };

            // Parse variant options from variant name
            var options = ParseVariantOptions(row);
            foreach (var option in options)
            {
                variant.Options.Add(option);
            }

            variants.Add(variant);
        }

        return variants;
    }

    private static decimal? ExtractVariantCurrentPrice(IElement row)
    {
        var element = row.QuerySelector("div.price-final[data-testid='productVariantPrice']");
        return ParsePrice(element?.TextContent);
    }

    private static decimal? ExtractVariantBasicPrice(IElement row)
    {
        var element = row.QuerySelector("span.price-standard > span");
        return ParsePrice(element?.TextContent);
    }

    private static string? ExtractVariantStock(IElement row)
    {
        // Stock is a span sibling of the variant-name div
        var variantNameDiv = row.QuerySelector("div.variant-name[data-testid='productVariantName']");
        if (variantNameDiv?.NextElementSibling is { TagName: "SPAN" } stockSpan)
        {
            return stockSpan.TextContent.Trim();
        }

        // Alternative: look for any span after the variant name
        var parent = variantNameDiv?.ParentElement;
        if (parent != null)
        {
            var spans = parent.QuerySelectorAll("span");
            foreach (var span in spans)
            {
                var text = span.TextContent.Trim();
                if (!string.IsNullOrWhiteSpace(text) && !text.Contains("Kc", StringComparison.OrdinalIgnoreCase))
                {
                    return text;
                }
            }
        }

        return null;
    }

    private static List<ScrapedVariantOption> ParseVariantOptions(IElement row)
    {
        var options = new List<ScrapedVariantOption>();
        var variantNameElement = row.QuerySelector("div.variant-name[data-testid='productVariantName']");
        var variantText = variantNameElement?.TextContent.Trim();

        if (string.IsNullOrWhiteSpace(variantText))
        {
            return options;
        }

        // Parse "key: value, key: value" format
        // Handle commas inside values (e.g., "10, 20, 30mm")
        var pairs = SplitVariantPairs(variantText);

        foreach (var pair in pairs)
        {
            var colonIndex = pair.IndexOf(':');
            if (colonIndex > 0)
            {
                var name = pair[..colonIndex].Trim();
                var value = pair[(colonIndex + 1)..].Trim();

                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
                {
                    options.Add(new ScrapedVariantOption
                    {
                        OptionName = name,
                        OptionValue = value
                    });
                }
            }
        }

        return options;
    }

    /// <summary>
    /// Splits variant text into key-value pairs, handling commas inside values.
    /// E.g., "Barva: cervena, Velikost: 10, 20, 30mm" should split into:
    /// ["Barva: cervena", "Velikost: 10, 20, 30mm"]
    /// </summary>
    private static List<string> SplitVariantPairs(string text)
    {
        var pairs = new List<string>();
        var currentPair = new StringBuilder();
        var parts = text.Split(',');

        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i].Trim();

            if (currentPair.Length == 0)
            {
                currentPair.Append(part);
            }
            else if (part.Contains(':'))
            {
                // This is a new key-value pair, save current and start new
                pairs.Add(currentPair.ToString());
                currentPair.Clear();
                currentPair.Append(part);
            }
            else
            {
                // This is a continuation of the current value (comma inside value)
                currentPair.Append(", ");
                currentPair.Append(part);
            }
        }

        // Add the last pair
        if (currentPair.Length > 0)
        {
            pairs.Add(currentPair.ToString());
        }

        return pairs;
    }

    private static ScrapedVariant CreateFallbackVariant(IDocument document)
    {
        return new ScrapedVariant
        {
            CurrentPrice = ExtractFallbackCurrentPrice(document),
            BasicPrice = ExtractFallbackBasicPrice(document),
            StockStatus = ExtractFallbackStock(document)
        };
    }

    private static decimal? ExtractFallbackCurrentPrice(IDocument document)
    {
        var element = document.QuerySelector("div.p-final-price-wrapper span.price-final-holder");
        return ParsePrice(element?.TextContent);
    }

    private static decimal? ExtractFallbackBasicPrice(IDocument document)
    {
        var element = document.QuerySelector("div.p-final-price-wrapper span.price-standard > span");
        return ParsePrice(element?.TextContent);
    }

    private static string? ExtractFallbackStock(IDocument document)
    {
        var element = document.QuerySelector("span.availability-label");
        return element?.TextContent.Trim();
    }

    /// <summary>
    /// Parses price from text like "XXX Kc" or "1 234 Kc" to decimal.
    /// Handles Czech price format with spaces as thousands separators.
    /// </summary>
    private static decimal? ParsePrice(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        // Extract numeric part using regex
        var match = PriceRegex.Match(text);
        if (!match.Success)
        {
            return null;
        }

        // Remove spaces and other non-numeric characters except decimal separator
        var numericText = match.Groups[1].Value
            .Replace(" ", "")
            .Replace("\u00A0", "") // non-breaking space
            .Replace(",", "."); // Czech decimal separator to standard

        if (decimal.TryParse(numericText, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out var price))
        {
            return price;
        }

        return null;
    }

    [GeneratedRegex(@"([\d\s\u00A0,\.]+)\s*(?:Kc|CZK|,-)?", RegexOptions.IgnoreCase)]
    private static partial Regex GeneratePriceRegex();
}
