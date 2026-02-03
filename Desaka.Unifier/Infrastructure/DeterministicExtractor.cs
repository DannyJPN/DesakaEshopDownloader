using System.Text.RegularExpressions;

namespace Desaka.Unifier.Infrastructure;

/// <summary>
/// Extracts property values using deterministic rules.
/// </summary>
public interface IDeterministicExtractor
{
    string? Extract(PropertyResolutionContext context);
}

public sealed class DeterministicExtractor : IDeterministicExtractor
{
    private static readonly Dictionary<string, Regex[]> PropertyPatterns = new()
    {
        ["brand"] = new[]
        {
            new Regex(@"^(Butterfly|Donic|Joola|Tibhar|Stiga|Nittaku|Yasaka|Xiom|DHS|Andro|Victas)\b", RegexOptions.IgnoreCase),
            new Regex(@"\b(Butterfly|Donic|Joola|Tibhar|Stiga|Nittaku|Yasaka|Xiom|DHS|Andro|Victas)\s", RegexOptions.IgnoreCase),
        },
        ["type"] = new[]
        {
            new Regex(@"\b(pálka|blade|potah|rubber|míček|ball|stůl|table|síť|net|taška|bag)\b", RegexOptions.IgnoreCase),
        },
        ["color"] = new[]
        {
            new Regex(@"\b(červen[áý]|modr[áý]|zelen[áý]|čern[áý]|bíl[áý]|žlut[áý]|oranžov[áý])\b", RegexOptions.IgnoreCase),
            new Regex(@"\b(red|blue|green|black|white|yellow|orange)\b", RegexOptions.IgnoreCase),
        },
        ["thickness"] = new[]
        {
            new Regex(@"\b(\d+[,.]?\d*)\s*mm\b", RegexOptions.IgnoreCase),
        },
    };

    public string? Extract(PropertyResolutionContext context)
    {
        var propertyKey = context.PropertyName.ToLowerInvariant();

        if (!PropertyPatterns.TryGetValue(propertyKey, out var patterns))
            return null;

        var textsToSearch = new List<string>();
        if (!string.IsNullOrEmpty(context.ProductName))
            textsToSearch.Add(context.ProductName);
        if (!string.IsNullOrEmpty(context.ProductUrl))
            textsToSearch.Add(context.ProductUrl);

        var allText = string.Join(" ", textsToSearch);

        foreach (var pattern in patterns)
        {
            var match = pattern.Match(allText);
            if (match.Success)
            {
                var value = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
                return value.Trim();
            }
        }

        return null;
    }
}
