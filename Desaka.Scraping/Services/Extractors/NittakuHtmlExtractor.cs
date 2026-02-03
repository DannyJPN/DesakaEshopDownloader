using System.Text;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services.Extractors;

/// <summary>
/// HTML extractor for Nittaku products from nittaku.tt (Shopify store)
/// </summary>
public sealed class NittakuHtmlExtractor : IExtractor
{
    private const string BaseUrl = "https://nittaku.tt";

    public async Task<ScrapedProduct> ExtractAsync(ScrapeContent content, CancellationToken cancellationToken = default)
    {
        var html = Encoding.UTF8.GetString(content.Body);
        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html, cancellationToken);

        var product = new ScrapedProduct
        {
            Url = ExtractCanonicalUrl(document) ?? content.Url,
            Name = ExtractName(document),
            ShortDescription = ExtractShortDescription(document),
            Description = ExtractDescription(document),
            MainPhotoUrl = ExtractMainPhoto(document)
        };

        ExtractGalleryPhotos(document, product);
        ExtractVariants(document, product);

        return product;
    }

    private static string? ExtractCanonicalUrl(IDocument doc) =>
        doc.QuerySelector("link[rel='canonical']")?.GetAttribute("href");

    private static string ExtractName(IDocument doc)
    {
        // Try h1 with product-meta__title class
        var h1 = doc.QuerySelectorAll("h1").FirstOrDefault(h =>
            h.ClassList.Any(c => c.Contains("product-meta__title")));
        if (h1 != null) return h1.TextContent.Trim();

        // Fallback to any h1
        h1 = doc.QuerySelector("h1");
        if (h1 != null) return h1.TextContent.Trim();

        // Try meta og:title
        var meta = doc.QuerySelector("meta[property='og:title']");
        if (meta != null)
        {
            var content = meta.GetAttribute("content") ?? "";
            var parts = content.Split(new[] { " – ", " | " }, StringSplitOptions.None);
            return parts[0].Trim();
        }

        return doc.QuerySelector("title")?.TextContent.Split(new[] { " – ", " | " }, StringSplitOptions.None)[0].Trim() ?? "";
    }

    private static string? ExtractShortDescription(IDocument doc)
    {
        var descDiv = doc.QuerySelector("div.rte.text--pull");
        var h1 = descDiv?.QuerySelector("h1");
        if (h1 != null) return h1.OuterHtml.Replace("\n", "<br>").Replace("\r", "");
        var p = descDiv?.QuerySelector("p");
        if (p != null) return p.OuterHtml.Replace("\n", "<br>").Replace("\r", "");
        var meta = doc.QuerySelector("meta[property='og:description']");
        if (meta != null) return $"<p>{meta.GetAttribute("content")?.Replace("\n", "<br>")}</p>";
        return null;
    }

    private static string? ExtractDescription(IDocument doc)
    {
        var descDiv = doc.QuerySelector("div.rte.text--pull")
            ?? doc.QuerySelector("div.product-description")
            ?? doc.QuerySelector("div.description");
        return descDiv?.InnerHtml.Replace("\n", "<br>").Replace("\r", "");
    }

    private static string? ExtractMainPhoto(IDocument doc)
    {
        var meta = doc.QuerySelector("meta[property='og:image']");
        var url = meta?.GetAttribute("content");
        return UpscaleImageUrl(url);
    }

    private static string? UpscaleImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        if (url.StartsWith("//")) url = "https:" + url;
        var sizes = new[] { "_800x", "_600x", "_500x", "_400x", "_300x", "_200x", "_130x", "_100x", "_60x" };
        foreach (var size in sizes)
            if (url.Contains(size)) return url.Replace(size, "_1600x");
        return url;
    }

    private static void ExtractGalleryPhotos(IDocument doc, ScrapedProduct product)
    {
        var mainUrl = product.MainPhotoUrl;
        foreach (var img in doc.QuerySelectorAll("img"))
        {
            var src = img.GetAttribute("src");
            if (string.IsNullOrWhiteSpace(src)) continue;
            if (src.Contains("icon") || src.Contains("logo") || src.Contains("payment") || src.Contains("social")) continue;
            var upscaled = UpscaleImageUrl(src);
            if (upscaled != mainUrl && !string.IsNullOrWhiteSpace(upscaled) && !product.GalleryUrls.Contains(upscaled))
                product.GalleryUrls.Add(upscaled);
        }
    }

    private static void ExtractVariants(IDocument doc, ScrapedProduct product)
    {
        var dimensions = new Dictionary<string, List<string>>();
        var selectors = doc.QuerySelectorAll("div.product-form__option");

        foreach (var selector in selectors)
        {
            if (selector.QuerySelector("select") != null) continue; // Skip dropdowns

            var nameElem = selector.QuerySelector("span.product-form__option-name");
            if (nameElem == null) continue;

            var rawName = nameElem.TextContent.Trim();
            var optionName = rawName.Contains(':') ? rawName.Split(':')[0].Trim() : rawName;
            if (string.IsNullOrEmpty(optionName)) continue;

            var values = new List<string>();
            foreach (var input in selector.QuerySelectorAll("input[type='radio']"))
            {
                var id = input.GetAttribute("id");
                if (string.IsNullOrEmpty(id)) continue;
                var label = selector.QuerySelector($"label[for='{id}']");
                if (label == null) continue;
                var textSpan = label.QuerySelector("span.block-swatch__item-text");
                var value = textSpan?.TextContent.Trim() ?? label.GetAttribute("title")?.Trim() ?? label.TextContent.Trim();
                if (!string.IsNullOrEmpty(value)) values.Add(value);
            }

            if (values.Count > 0) dimensions[optionName] = values;
        }

        var stock = doc.QuerySelector("span.product-form__inventory")?.TextContent.Trim() ?? "sofort versandbereit";

        if (dimensions.Count == 0)
        {
            product.Variants.Add(new ScrapedVariant { CurrentPrice = 0, BasicPrice = 0, StockStatus = stock });
            return;
        }

        foreach (var combo in CartesianProduct(dimensions))
        {
            var variant = new ScrapedVariant { CurrentPrice = 0, BasicPrice = 0, StockStatus = stock };
            foreach (var kvp in combo)
                variant.Options.Add(new ScrapedVariantOption { OptionName = kvp.Key, OptionValue = kvp.Value });
            product.Variants.Add(variant);
        }
    }

    private static IEnumerable<Dictionary<string, string>> CartesianProduct(Dictionary<string, List<string>> dims)
    {
        IEnumerable<Dictionary<string, string>> result = new[] { new Dictionary<string, string>() };
        foreach (var (key, vals) in dims)
            result = result.SelectMany(d => vals.Select(v => new Dictionary<string, string>(d) { [key] = v }));
        return result;
    }
}
