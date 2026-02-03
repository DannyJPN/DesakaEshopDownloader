using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services.Extractors;

/// <summary>
/// HTML extractor for SportSpin products from sportspin.cz
/// </summary>
public sealed class SportSpinHtmlExtractor : IExtractor
{
    public async Task<ScrapedProduct> ExtractAsync(ScrapeContent content, CancellationToken cancellationToken = default)
    {
        var html = Encoding.UTF8.GetString(content.Body);
        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html, cancellationToken);

        var product = new ScrapedProduct
        {
            Url = ExtractUrl(document) ?? content.Url,
            Name = ExtractName(document),
            ShortDescription = ExtractShortDescription(document),
            Description = ExtractDescription(document),
            MainPhotoUrl = ExtractMainPhoto(document)
        };

        ExtractGalleryPhotos(document, product);
        ExtractVariants(document, product);

        return product;
    }

    private static string? ExtractUrl(IDocument doc) =>
        doc.QuerySelector("meta[property='og:url']")?.GetAttribute("content");

    private static string ExtractName(IDocument doc)
    {
        var header = doc.QuerySelector("div.p-detail-inner-header");
        return header?.QuerySelector("h1")?.TextContent.Trim() ?? "";
    }

    private static string? ExtractShortDescription(IDocument doc)
    {
        var div = doc.QuerySelector("div.p-short-description[data-testid='productCardShortDescr']");
        return div?.InnerHtml.Replace("\n", " ").Replace("\r", "").Trim();
    }

    private static string? ExtractDescription(IDocument doc)
    {
        var div = doc.QuerySelector("div.basic-description");
        return div?.InnerHtml.Replace("\n", " ").Replace("\r", "").Trim();
    }

    private static string? ExtractMainPhoto(IDocument doc) =>
        doc.QuerySelector("a.p-main-image.cloud-zoom")?.GetAttribute("data-href");

    private static void ExtractGalleryPhotos(IDocument doc, ScrapedProduct product)
    {
        foreach (var a in doc.QuerySelectorAll("a.p-thumbnail"))
        {
            var href = a.GetAttribute("href");
            if (!string.IsNullOrWhiteSpace(href) && !product.GalleryUrls.Contains(href))
                product.GalleryUrls.Add(href);
        }
    }

    private static void ExtractVariants(IDocument doc, ScrapedProduct product)
    {
        var variantTags = doc.QuerySelectorAll("div.table-row[data-testid='productVariant']");

        if (variantTags.Length > 0)
        {
            foreach (var variantTag in variantTags)
            {
                var variant = new ScrapedVariant();

                // Parse variant name: "Barva: červená, Velikost: XL"
                var nameTag = variantTag.QuerySelector("div.variant-name[data-testid='productVariantName']");
                if (nameTag != null)
                {
                    var pairs = ParseKeyValuePairs(nameTag.TextContent);
                    foreach (var kvp in pairs)
                        variant.Options.Add(new ScrapedVariantOption { OptionName = kvp.Key, OptionValue = kvp.Value });

                    // Stock is sibling span
                    var stockSpan = nameTag.NextElementSibling;
                    if (stockSpan?.TagName == "SPAN")
                        variant.StockStatus = stockSpan.TextContent.Trim();
                }

                // Current price
                var priceTag = variantTag.QuerySelector("div.price-final[data-testid='productVariantPrice']");
                variant.CurrentPrice = ParseCzkPrice(priceTag?.TextContent);

                // Basic price
                var basicPriceTag = variantTag.QuerySelector("span.price-standard")?.QuerySelector("span");
                variant.BasicPrice = ParseCzkPrice(basicPriceTag?.TextContent) ?? variant.CurrentPrice;

                product.Variants.Add(variant);
            }
        }
        else
        {
            // Fallback: single variant
            var (basicPrice, currentPrice) = ExtractFallbackPrices(doc);
            var stock = doc.QuerySelector("span.availability-label")?.TextContent.Trim();
            product.Variants.Add(new ScrapedVariant { CurrentPrice = currentPrice, BasicPrice = basicPrice, StockStatus = stock });
        }
    }

    private static Dictionary<string, string> ParseKeyValuePairs(string text)
    {
        var result = new Dictionary<string, string>();
        var parts = new List<string>();
        var current = "";

        // Split by comma but handle values that contain commas
        foreach (var part in text.Split(','))
        {
            if (part.Contains(':'))
            {
                if (!string.IsNullOrEmpty(current))
                    parts.Add(current.Trim());
                current = part;
            }
            else
            {
                current += "," + part;
            }
        }
        if (!string.IsNullOrEmpty(current))
            parts.Add(current.Trim());

        foreach (var pair in parts)
        {
            var colonIdx = pair.IndexOf(':');
            if (colonIdx > 0)
            {
                var key = pair[..colonIdx].Trim();
                var value = pair[(colonIdx + 1)..].Trim();
                result[key] = value;
            }
        }

        return result;
    }

    private static decimal? ParseCzkPrice(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var cleaned = text.Replace(" ", "").Replace("Kč", "").Replace(",", ".").Trim();
        return decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var price) ? price : null;
    }

    private static (decimal? basicPrice, decimal? currentPrice) ExtractFallbackPrices(IDocument doc)
    {
        var wrapper = doc.QuerySelector("div.p-final-price-wrapper");
        var currentTag = wrapper?.QuerySelector("span.price-final-holder");
        var currentPrice = ParseCzkPrice(currentTag?.TextContent);

        var basicTag = wrapper?.QuerySelector("span.price-standard")?.QuerySelector("span");
        var basicPrice = ParseCzkPrice(basicTag?.TextContent) ?? currentPrice;

        return (basicPrice, currentPrice);
    }
}
