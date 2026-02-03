using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services.Extractors;

/// <summary>
/// HTML extractor for VseNaStolniTenis products from vsenastolnitenis.cz
/// This extractor handles JavaScript-based variant data.
/// </summary>
public sealed class VseNaStolniTenisHtmlExtractor : IExtractor
{
    private const string BaseUrl = "https://www.vsenastolnitenis.cz";

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
        ExtractVariants(document, html, product);

        return product;
    }

    private static string? ExtractUrl(IDocument doc)
    {
        var baseTag = doc.QuerySelector("base");
        return baseTag?.GetAttribute("href") ?? BaseUrl;
    }

    private static string ExtractName(IDocument doc)
    {
        var h1 = doc.QuerySelector("h1.pp-dash");
        if (h1 == null) return "";
        var text = string.Join("", h1.ChildNodes.Where(n => n.NodeType == NodeType.Text).Select(n => n.TextContent));
        return Regex.Replace(text, "<[^>]+>", "").Trim();
    }

    private static string? ExtractShortDescription(IDocument doc)
    {
        var span = doc.QuerySelector("span.sdesc");
        return span?.InnerHtml.Replace("\n", " ").Replace("\r", "").Trim();
    }

    private static string? ExtractDescription(IDocument doc)
    {
        var div = doc.QuerySelector("div#collapseOne.collapse.show");
        return div?.InnerHtml.Replace("\n", " ").Replace("\r", "").Trim();
    }

    private static string? ExtractMainPhoto(IDocument doc)
    {
        var img = doc.QuerySelector("img.myzoom.img-fluid.m-auto");
        var src = img?.GetAttribute("src");
        return string.IsNullOrWhiteSpace(src) ? null : EnsureAbsoluteUrl(src);
    }

    private static void ExtractGalleryPhotos(IDocument doc, ScrapedProduct product)
    {
        foreach (var picture in doc.QuerySelectorAll("picture"))
        {
            var source = picture.QuerySelector("source");
            var srcset = source?.GetAttribute("srcset");
            if (!string.IsNullOrWhiteSpace(srcset))
            {
                var url = EnsureAbsoluteUrl(srcset);
                if (!product.GalleryUrls.Contains(url))
                    product.GalleryUrls.Add(url);
            }
        }
    }

    private static string EnsureAbsoluteUrl(string url)
    {
        if (url.StartsWith("http://") || url.StartsWith("https://"))
            return url;
        return BaseUrl + (url.StartsWith("/") ? url : "/" + url);
    }

    private static void ExtractVariants(IDocument doc, string html, ScrapedProduct product)
    {
        // Extract HTML variant options
        var dimensions = ExtractHtmlVariantDimensions(doc);

        // Extract JavaScript variant data
        var jsVariants = ExtractJsVariants(html);

        if (dimensions.Count == 0)
        {
            // No variants - create single variant
            var firstJs = jsVariants.FirstOrDefault();
            product.Variants.Add(new ScrapedVariant
            {
                CurrentPrice = firstJs?.PriceRaw,
                BasicPrice = firstJs?.PriceOldRaw ?? firstJs?.PriceRaw,
                StockStatus = firstJs?.AvailabilityTxt
            });
            return;
        }

        // Get all radio inputs for matching to JS data
        var allInputs = doc.QuerySelectorAll("div.mb-2.pp-detail-options input[type='radio']")
            .Select(i => new { Name = i.GetAttribute("name") ?? "", Value = i.GetAttribute("value") ?? "",
                Text = i.ParentElement?.TextContent.Trim() ?? "" })
            .ToList();

        // Generate all combinations and match with JS data
        foreach (var combo in CartesianProduct(dimensions))
        {
            var variant = new ScrapedVariant();

            // Build search criteria for JS matching
            var searchCriteria = new Dictionary<string, string>();
            foreach (var (optName, optValue) in combo)
            {
                variant.Options.Add(new ScrapedVariantOption { OptionName = optName, OptionValue = optValue });

                // Find matching input value
                var matchingInput = allInputs.FirstOrDefault(i => i.Name == optName && i.Text == optValue);
                if (matchingInput != null)
                    searchCriteria[optName.ToLowerInvariant()] = matchingInput.Value;
            }

            // Find matching JS variant
            var jsMatch = jsVariants.FirstOrDefault(js =>
                searchCriteria.All(kv => js.Properties.TryGetValue(kv.Key, out var v) && v == kv.Value));

            variant.CurrentPrice = jsMatch?.PriceRaw;
            variant.BasicPrice = jsMatch?.PriceOldRaw ?? jsMatch?.PriceRaw;
            variant.StockStatus = jsMatch?.AvailabilityTxt;

            product.Variants.Add(variant);
        }
    }

    private static Dictionary<string, List<string>> ExtractHtmlVariantDimensions(IDocument doc)
    {
        var dimensions = new Dictionary<string, List<string>>();

        foreach (var div in doc.QuerySelectorAll("div.mb-2.pp-detail-options"))
        {
            var inputs = div.QuerySelectorAll("input[type='radio']");
            var values = new List<(string Name, string Value)>();

            foreach (var input in inputs)
            {
                var name = input.GetAttribute("name") ?? "";
                var value = input.ParentElement?.TextContent.Trim() ?? "";
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                    values.Add((name, value));
            }

            foreach (var group in values.GroupBy(v => v.Name))
            {
                if (!dimensions.ContainsKey(group.Key))
                    dimensions[group.Key] = new List<string>();
                dimensions[group.Key].AddRange(group.Select(g => g.Value).Distinct());
            }
        }

        return dimensions;
    }

    private static List<JsVariant> ExtractJsVariants(string html)
    {
        var results = new List<JsVariant>();

        try
        {
            var match = Regex.Match(html, @"var\s+product_variants\s*=\s*(\[.*?\]);", RegexOptions.Singleline);
            if (!match.Success) return results;

            var jsArray = match.Groups[1].Value;

            // Convert JS to valid JSON
            // Replace single quotes with double quotes
            jsArray = jsArray.Replace("'", "\"");
            // Quote unquoted property names
            jsArray = Regex.Replace(jsArray, @"(\w+)\s*:", "\"$1\":");
            // Handle trailing commas
            jsArray = Regex.Replace(jsArray, @",\s*([}\]])", "$1");

            using var jsonDoc = JsonDocument.Parse(jsArray);
            foreach (var element in jsonDoc.RootElement.EnumerateArray())
            {
                var variant = new JsVariant();

                foreach (var prop in element.EnumerateObject())
                {
                    var key = prop.Name.ToLowerInvariant();
                    switch (key)
                    {
                        case "price_raw":
                            variant.PriceRaw = prop.Value.TryGetDecimal(out var pr) ? pr : null;
                            break;
                        case "priceold_raw":
                            variant.PriceOldRaw = prop.Value.TryGetDecimal(out var por) ? por : null;
                            break;
                        case "availability_txt":
                            variant.AvailabilityTxt = prop.Value.GetString();
                            break;
                        default:
                            variant.Properties[key] = prop.Value.GetString() ?? "";
                            break;
                    }
                }

                results.Add(variant);
            }
        }
        catch
        {
            // JSON parsing failed - return empty list
        }

        return results;
    }

    private static IEnumerable<Dictionary<string, string>> CartesianProduct(Dictionary<string, List<string>> dims)
    {
        IEnumerable<Dictionary<string, string>> result = new[] { new Dictionary<string, string>() };
        foreach (var (key, vals) in dims)
            result = result.SelectMany(d => vals.Select(v => new Dictionary<string, string>(d) { [key] = v }));
        return result;
    }

    private class JsVariant
    {
        public decimal? PriceRaw { get; set; }
        public decimal? PriceOldRaw { get; set; }
        public string? AvailabilityTxt { get; set; }
        public Dictionary<string, string> Properties { get; } = new();
    }
}
