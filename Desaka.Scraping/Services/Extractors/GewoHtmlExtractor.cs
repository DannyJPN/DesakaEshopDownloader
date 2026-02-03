using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using Desaka.Scraping.Models;

namespace Desaka.Scraping.Services.Extractors;

/// <summary>
/// HTML extractor for GEWO products from contra.de
/// </summary>
public sealed class GewoHtmlExtractor : IExtractor
{
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

    private static string? ExtractCanonicalUrl(IDocument doc)
    {
        return doc.QuerySelector("link[rel='canonical']")?.GetAttribute("href");
    }

    private static string ExtractName(IDocument doc)
    {
        return doc.QuerySelector("h1.product-detail-buy__name")?.TextContent.Trim() ?? "";
    }

    private static string? ExtractShortDescription(IDocument doc)
    {
        var descDiv = doc.QuerySelector("div.product-detail-description-text");
        var firstP = descDiv?.QuerySelector("p");
        return firstP?.InnerHtml.Replace("\n", " ").Replace("\r", "").Trim();
    }

    private static string? ExtractDescription(IDocument doc)
    {
        var descDiv = doc.QuerySelector("div.product-detail-description-text");
        return descDiv?.InnerHtml.Replace("\n", " ").Replace("\r", "").Trim();
    }

    private static string? ExtractMainPhoto(IDocument doc)
    {
        var gallery = doc.QuerySelector("div.base-slider.gallery-slider");
        return gallery?.QuerySelector("img")?.GetAttribute("src");
    }

    private static void ExtractGalleryPhotos(IDocument doc, ScrapedProduct product)
    {
        var gallery = doc.QuerySelector("div.base-slider.gallery-slider");
        if (gallery == null) return;

        var imgs = gallery.QuerySelectorAll("img").Skip(1); // Skip main photo
        foreach (var img in imgs)
        {
            var src = img.GetAttribute("src");
            if (!string.IsNullOrWhiteSpace(src))
                product.GalleryUrls.Add(src);
        }
    }

    private static void ExtractVariants(IDocument doc, ScrapedProduct product)
    {
        // Try first method: productVariant divs
        var variantDivs = doc.QuerySelectorAll("div[data-testid='productVariant']");
        if (variantDivs.Length > 0)
        {
            ExtractVariantsFromProductVariant(variantDivs, product);
            return;
        }

        // Try second method: configurator groups
        var groups = doc.QuerySelectorAll("div.product-detail-configurator-group");
        if (groups.Length > 0)
        {
            ExtractVariantsFromConfiguratorGroups(doc, groups, product);
            return;
        }

        // Fallback: single variant with price
        var price = ExtractPrice(doc);
        var variant = new ScrapedVariant { CurrentPrice = price, BasicPrice = price };
        product.Variants.Add(variant);
    }

    private static void ExtractVariantsFromProductVariant(IHtmlCollection<IElement> variantDivs, ScrapedProduct product)
    {
        foreach (var variantDiv in variantDivs)
        {
            var variant = new ScrapedVariant();

            // Extract key-value pairs from groups
            var groups = variantDiv.QuerySelectorAll("div.product-detail-buy__group");
            foreach (var group in groups)
            {
                var key = group.TextContent.Trim().TrimEnd(':');
                var checkedInputs = group.QuerySelectorAll("li.tile-select__list-item input.product-detail-configurator-option-input[checked]");
                foreach (var input in checkedInputs)
                {
                    var label = input.ParentElement?.QuerySelector("label");
                    if (label != null)
                    {
                        variant.Options.Add(new ScrapedVariantOption
                        {
                            OptionName = key,
                            OptionValue = label.TextContent.Trim()
                        });
                    }
                }
            }

            // Extract price
            var priceTag = variantDiv.QuerySelector("div.product-price__price");
            variant.CurrentPrice = ParsePrice(priceTag?.TextContent);
            variant.BasicPrice = variant.CurrentPrice;

            product.Variants.Add(variant);
        }
    }

    private static void ExtractVariantsFromConfiguratorGroups(IDocument doc, IHtmlCollection<IElement> groups, ScrapedProduct product)
    {
        var variantDimensions = new Dictionary<string, List<string>>();

        foreach (var group in groups)
        {
            var nameTag = group.QuerySelector("div.product-detail-buy__group");
            if (nameTag == null) continue;

            var groupName = nameTag.TextContent.Trim().Replace(":", "");
            var values = new List<string>();

            var items = group.QuerySelectorAll("li.tile-select__list-item, li.tile-select__list-item--is-text");
            foreach (var item in items)
            {
                var label = item.QuerySelector("label");
                var text = label?.GetAttribute("title") ?? label?.TextContent.Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    var span = item.QuerySelector("span.tile-select__preview--is-text");
                    text = span?.TextContent.Trim();
                }
                if (!string.IsNullOrWhiteSpace(text))
                    values.Add(text);
            }

            if (values.Count > 0)
                variantDimensions[groupName] = values;
        }

        var price = ExtractPrice(doc);
        var stock = doc.QuerySelector("div.delivery-information")?.TextContent.Trim();

        // Generate Cartesian product of all variants
        if (variantDimensions.Count > 0)
        {
            var combinations = CartesianProduct(variantDimensions);
            foreach (var combo in combinations)
            {
                var variant = new ScrapedVariant
                {
                    CurrentPrice = price,
                    BasicPrice = price,
                    StockStatus = stock
                };
                foreach (var kvp in combo)
                {
                    variant.Options.Add(new ScrapedVariantOption { OptionName = kvp.Key, OptionValue = kvp.Value });
                }
                product.Variants.Add(variant);
            }
        }
        else
        {
            product.Variants.Add(new ScrapedVariant { CurrentPrice = price, BasicPrice = price, StockStatus = stock });
        }
    }

    private static decimal? ExtractPrice(IDocument doc)
    {
        var priceTag = doc.QuerySelector("div.product-detail-price");
        return ParsePrice(priceTag?.TextContent);
    }

    private static decimal? ParsePrice(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var match = Regex.Match(text, @"(\d+[,\.]\d+)");
        if (match.Success)
        {
            var priceStr = match.Groups[1].Value.Replace(',', '.');
            if (decimal.TryParse(priceStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var price))
                return price;
        }
        return null;
    }

    private static IEnumerable<Dictionary<string, string>> CartesianProduct(Dictionary<string, List<string>> dimensions)
    {
        var keys = dimensions.Keys.ToList();
        var values = dimensions.Values.ToList();

        IEnumerable<Dictionary<string, string>> result = new[] { new Dictionary<string, string>() };

        for (var i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            var vals = values[i];
            result = result.SelectMany(dict => vals.Select(v =>
            {
                var newDict = new Dictionary<string, string>(dict) { [key] = v };
                return newDict;
            }));
        }

        return result;
    }
}
