namespace Desaka.Comparation;

/// <summary>
/// Interface for fuzzy string matching operations.
/// </summary>
public interface IFuzzyMatcher
{
    /// <summary>
    /// Calculate similarity ratio between two strings.
    /// </summary>
    double GetSimilarity(string a, string b);

    /// <summary>
    /// Find the best matching candidate above threshold.
    /// </summary>
    MatchResult? FindBestMatch(string query, IEnumerable<string> candidates, double threshold = 0.9);

    /// <summary>
    /// Find all matching candidates above threshold.
    /// </summary>
    IEnumerable<MatchResult> FindAllMatches(string query, IEnumerable<string> candidates, double threshold = 0.9);

    /// <summary>
    /// Find best match in a dictionary, returning the associated value.
    /// </summary>
    MatchResult? FindBestMatchInDictionary(string query, IDictionary<string, string> dictionary, double threshold = 0.9);
}

/// <summary>
/// Result of a fuzzy match operation.
/// </summary>
public record MatchResult(string Key, string Value, double Score);
