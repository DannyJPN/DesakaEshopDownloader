using System.Text.RegularExpressions;

namespace Desaka.Unifier.Infrastructure;

/// <summary>
/// Extracts property values using heuristic rules based on product name, URL, and description.
/// </summary>
public interface IHeuristicExtractor
{
    string? Extract(PropertyResolutionContext context);
}

public sealed class HeuristicExtractor : IHeuristicExtractor
{
    public string? Extract(PropertyResolutionContext context)
    {
        if (context.CandidateValues == null || context.CandidateValues.Count == 0)
            return null;

        var textsToSearch = new List<string>();

        if (!string.IsNullOrEmpty(context.ProductName))
            textsToSearch.Add(context.ProductName);
        if (!string.IsNullOrEmpty(context.ProductUrl))
            textsToSearch.Add(context.ProductUrl);
        if (!string.IsNullOrEmpty(context.Description))
            textsToSearch.Add(context.Description);
        if (!string.IsNullOrEmpty(context.ShortDescription))
            textsToSearch.Add(context.ShortDescription);

        var allText = string.Join(" ", textsToSearch).ToLowerInvariant();

        // Find exact matches in text (whole words only)
        var matches = new List<(string Value, int Count)>();

        foreach (var candidate in context.CandidateValues)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            var pattern = @"\b" + Regex.Escape(candidate.ToLowerInvariant()) + @"\b";
            var matchCount = Regex.Matches(allText, pattern).Count;

            if (matchCount > 0)
                matches.Add((candidate, matchCount));
        }

        // Return single match if exactly one found
        if (matches.Count == 1)
            return matches[0].Value;

        // If multiple matches, return the one with highest frequency
        if (matches.Count > 1)
        {
            var best = matches.OrderByDescending(m => m.Count).First();
            // Only return if significantly better than others
            var secondBest = matches.OrderByDescending(m => m.Count).Skip(1).First();
            if (best.Count > secondBest.Count * 2)
                return best.Value;
        }

        return null;
    }
}
