namespace Desaka.Comparation;

/// <summary>
/// Fuzzy string matcher using SequenceMatcher-like algorithm (Python difflib equivalent).
/// </summary>
public sealed class FuzzyMatcher : IFuzzyMatcher
{
    public double GetSimilarity(string a, string b)
    {
        if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return 1.0;
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0.0;

        var na = TextNormalizer.ToSearchForm(a);
        var nb = TextNormalizer.ToSearchForm(b);
        if (na == nb) return 1.0;

        var matches = CountMatchingBlocks(na, nb);
        return 2.0 * matches / (na.Length + nb.Length);
    }

    private static int CountMatchingBlocks(string a, string b)
    {
        if (a.Length == 0 || b.Length == 0) return 0;
        var totalMatches = 0;
        var usedA = new bool[a.Length];
        var usedB = new bool[b.Length];

        while (true)
        {
            var (start1, start2, length) = FindLongestMatch(a, b, usedA, usedB);
            if (length == 0) break;
            totalMatches += length;
            for (var i = 0; i < length; i++) { usedA[start1 + i] = true; usedB[start2 + i] = true; }
        }
        return totalMatches;
    }

    private static (int, int, int) FindLongestMatch(string a, string b, bool[] usedA, bool[] usedB)
    {
        int bestStart1 = 0, bestStart2 = 0, bestLength = 0;
        for (var i = 0; i < a.Length; i++)
        {
            if (usedA[i]) continue;
            for (var j = 0; j < b.Length; j++)
            {
                if (usedB[j]) continue;
                var length = 0;
                while (i + length < a.Length && j + length < b.Length &&
                       !usedA[i + length] && !usedB[j + length] && a[i + length] == b[j + length])
                    length++;
                if (length > bestLength) { bestStart1 = i; bestStart2 = j; bestLength = length; }
            }
        }
        return (bestStart1, bestStart2, bestLength);
    }

    public MatchResult? FindBestMatch(string query, IEnumerable<string> candidates, double threshold = 0.9)
    {
        if (string.IsNullOrEmpty(query)) return null;
        MatchResult? best = null;
        foreach (var c in candidates)
        {
            if (string.IsNullOrEmpty(c)) continue;
            var score = GetSimilarity(query, c);
            if (score >= threshold && (best is null || score > best.Score))
                best = new MatchResult(c, c, score);
        }
        return best;
    }

    public IEnumerable<MatchResult> FindAllMatches(string query, IEnumerable<string> candidates, double threshold = 0.9)
    {
        if (string.IsNullOrEmpty(query)) yield break;
        var results = new List<MatchResult>();
        foreach (var c in candidates)
        {
            if (string.IsNullOrEmpty(c)) continue;
            var score = GetSimilarity(query, c);
            if (score >= threshold) results.Add(new MatchResult(c, c, score));
        }
        results.Sort((a, b) => b.Score.CompareTo(a.Score));
        foreach (var r in results) yield return r;
    }

    public MatchResult? FindBestMatchInDictionary(string query, IDictionary<string, string> dictionary, double threshold = 0.9)
    {
        if (string.IsNullOrEmpty(query) || dictionary.Count == 0) return null;
        MatchResult? best = null;
        foreach (var kvp in dictionary)
        {
            if (string.IsNullOrEmpty(kvp.Key)) continue;
            var score = GetSimilarity(query, kvp.Key);
            if (score >= threshold && (best is null || score > best.Score))
                best = new MatchResult(kvp.Key, kvp.Value, score);
        }
        return best;
    }
}
