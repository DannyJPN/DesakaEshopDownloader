using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services.Extractors;

/// <summary>
/// HTML extractor for Spinway products from spinway.sk (WooCommerce)
/// </summary>
public sealed class SpinwayHtmlExtractor : IExtractor
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
        doc.QuerySelector("h1.product_title")?.TextContent.Trim() ?? "";

    private static string? ExtractShortDescription(IDocument doc)
    {
        var div = doc.QuerySelector("div.product-params-short");
        return div?.InnerHtml.Replace("\n", " ").Replace("\r", "").Trim();
    }

    private static string? ExtractDescription(IDocument doc)
    {
        var div = doc.QuerySelector("div.product-detail-description");
        return div?.InnerHtml.Replace("\n", " ").Replace("\r", "").Trim();
    }

    private static string? ExtractMainPhoto(IDocument doc)
    {
        var gallery = doc.QuerySelector("div.woocommerce-product-gallery__wrapper");
        return gallery?.QuerySelector("img")?.GetAttribute("src");
    }

    private static void ExtractGalleryPhotos(IDocument doc, ScrapedProduct product)
    {
        var imgs = doc.QuerySelectorAll("div.woocommerce-product-gallery__wrapper img");
        foreach (var img in imgs)
        {
            var src = img.GetAttribute("src");
            if (!string.IsNullOrWhiteSpace(src) && !product.GalleryUrls.Contains(src))
                product.GalleryUrls.Add(src);
        }
    }

    private static void ExtractVariants(IDocument doc, ScrapedProduct product)
    {
        var (basicPrice, currentPrice) = ExtractPrices(doc);
        var stock = ExtractStock(doc);

        var form = doc.QuerySelector("form.variations_form");
        if (form == null)
        {
            product.Variants.Add(new ScrapedVariant { CurrentPrice = currentPrice, BasicPrice = basicPrice, StockStatus = stock });
            return;
        }

        var selects = form.QuerySelectorAll("table.variations select");
        if (selects.Length == 0)
        {
            product.Variants.Add(new ScrapedVariant { CurrentPrice = currentPrice, BasicPrice = basicPrice, StockStatus = stock });
            return;
        }

        var dimensions = new Dictionary<string, List<string>>();
        foreach (var select in selects)
        {
            var name = select.GetAttribute("name")?.Replace("attribute_", "") ?? "";
            if (string.IsNullOrEmpty(name)) continue;

            var values = select.QuerySelectorAll("option")
                .Select(o => o.GetAttribute("value"))
                .Where(v => !string.IsNullOrEmpty(v))
                .Cast<string>()
                .ToList();

            if (values.Count > 0) dimensions[name] = values;
        }

        if (dimensions.Count == 0)
        {
            product.Variants.Add(new ScrapedVariant { CurrentPrice = currentPrice, BasicPrice = basicPrice, StockStatus = stock });
            return;
        }

        foreach (var combo in CartesianProduct(dimensions))
        {
            var variant = new ScrapedVariant { CurrentPrice = currentPrice, BasicPrice = basicPrice, StockStatus = stock };
            foreach (var kvp in combo)
                variant.Options.Add(new ScrapedVariantOption { OptionName = kvp.Key, OptionValue = kvp.Value });
            product.Variants.Add(variant);
        }
    }

    private static (decimal? basicPrice, decimal? currentPrice) ExtractPrices(IDocument doc)
    {
        var priceTag = doc.QuerySelector("p.price span.woocommerce-Price-amount");
        var text = priceTag?.TextContent ?? "";
        var price = ParseEuroPrice(text);
        return (price, price);
    }

    private static decimal? ParseEuroPrice(string text)
    {
        var cleaned = text.Replace("€", "").Replace(",", ".").Trim();
        return decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var price) ? price : null;
    }

    private static string? ExtractStock(IDocument doc) =>
        doc.QuerySelector("form.variations_form.cart p.stock")?.TextContent.Trim();

    private static IEnumerable<Dictionary<string, string>> CartesianProduct(Dictionary<string, List<string>> dims)
    {
        IEnumerable<Dictionary<string, string>> result = new[] { new Dictionary<string, string>() };
        foreach (var (key, vals) in dims)
            result = result.SelectMany(d => vals.Select(v => new Dictionary<string, string>(d) { [key] = v }));
        return result;
    }
}
