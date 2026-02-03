using System.Text;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services.Extractors;

/// <summary>
/// HTML extractor for Stoten products from stoten.cz
/// </summary>
public sealed class StotenHtmlExtractor : IExtractor
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

    private static string ExtractName(IDocument doc) =>
        doc.QuerySelector("h1[itemprop='name'][data-testid='textProductName']")?.TextContent.Trim() ?? "";

    private static string? ExtractShortDescription(IDocument doc)
    {
        var div = doc.QuerySelector("div.description-inner[data-testid='productCardDescr']");
        var p = div?.QuerySelector("p");
        return p?.InnerHtml.Replace("\n", " ").Replace("\r", "").Trim();
    }

    private static string? ExtractDescription(IDocument doc)
    {
        var div = doc.QuerySelector("div.description-inner[data-testid='productCardDescr']");
        return div?.InnerHtml.Replace("\n", " ").Replace("\r", "").Trim();
    }

    private static string? ExtractMainPhoto(IDocument doc) =>
        doc.QuerySelector("a#gallery-image")?.GetAttribute("href");

    private static void ExtractGalleryPhotos(IDocument doc, ScrapedProduct product)
    {
        foreach (var a in doc.QuerySelectorAll("a[data-gallery='lightbox[gallery]']"))
        {
            var href = a.GetAttribute("href");
            if (!string.IsNullOrWhiteSpace(href) && !product.GalleryUrls.Contains(href))
                product.GalleryUrls.Add(href);
        }
    }

    private static void ExtractVariants(IDocument doc, ScrapedProduct product)
    {
        var variantRows = doc.QuerySelectorAll("tr[data-testid='productVariant']");
        var allPrices = new List<decimal>();

        if (variantRows.Length > 0)
        {
            foreach (var row in variantRows)
            {
                var variant = new ScrapedVariant();

                // Parse variant name from td - contains br-separated key:value pairs
                var nameTd = row.QuerySelector("td.variant[data-testid='productVariantName']");
                if (nameTd != null)
                {
                    var rawContent = nameTd.InnerHtml
                        .Replace("<br />", "\n")
                        .Replace("<br/>", "\n")
                        .Replace("<br>", "\n");

                    // Strip HTML tags and parse
                    var textContent = System.Text.RegularExpressions.Regex.Replace(rawContent, "<[^>]+>", "");
                    foreach (var line in textContent.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var colonIdx = line.IndexOf(':');
                        if (colonIdx > 0)
                        {
                            var key = line[..colonIdx].Trim();
                            var value = line[(colonIdx + 1)..].Trim();
                            variant.Options.Add(new ScrapedVariantOption { OptionName = key, OptionValue = value });
                        }
                    }
                }

                // Current price
                var priceTd = row.QuerySelector("td.tari.variant-price[data-testid='productVariantPrice']");
                var strong = priceTd?.QuerySelector("strong");
                variant.CurrentPrice = ParseCzkPrice(strong?.TextContent);
                if (variant.CurrentPrice.HasValue)
                    allPrices.Add(variant.CurrentPrice.Value);

                // Stock status
                var stockTd = row.QuerySelector("td.variant-availability");
                var stockSpan = stockTd?.QuerySelector("span.show-tooltip.acronym");
                variant.StockStatus = stockSpan?.TextContent.Trim();

                product.Variants.Add(variant);
            }

            // Set basic price to highest price for all variants
            if (allPrices.Count > 0)
            {
                var maxPrice = allPrices.Max();
                foreach (var v in product.Variants)
                    v.BasicPrice = maxPrice;
            }
        }
        else
        {
            // Fallback: single variant
            var (basicPrice, currentPrice) = ExtractFallbackPrices(doc);
            product.Variants.Add(new ScrapedVariant { CurrentPrice = currentPrice, BasicPrice = basicPrice });
        }
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
        var currentTag = doc.QuerySelector("strong.price.sub-left-position[data-testid='productCardPrice']");
        var currentPrice = ParseCzkPrice(currentTag?.TextContent);

        var basicTag = doc.QuerySelector("td.td-normal-price")?.QuerySelector("span.line");
        var basicPrice = ParseCzkPrice(basicTag?.TextContent) ?? currentPrice;

        return (basicPrice, currentPrice);
    }
}
