using Desaka.Comparation;
using Microsoft.Extensions.Logging;

namespace Desaka.Unifier.Infrastructure;

/// <summary>
/// 5-Step decision tree for resolving product properties.
/// Order: 1. Exact Memory → 2. Fuzzy Match → 3. Heuristic → 4. Deterministic → 5. AI → 6. User Approval
/// </summary>
public interface IPropertyResolver
{
    Task<string?> ResolveAsync(PropertyResolutionContext context, CancellationToken ct = default);
}

public record PropertyResolutionContext(
    string PropertyName,
    string ProductName,
    string ProductUrl,
    string? Description,
    string? ShortDescription,
    IDictionary<string, string> MemoryDictionary,
    IReadOnlyList<string>? CandidateValues = null,
    bool AllowAi = true,
    bool RequireUserApproval = false
);

public sealed class PropertyResolver : IPropertyResolver
{
    private readonly IFuzzyMatcher _fuzzyMatcher;
    private readonly IHeuristicExtractor _heuristicExtractor;
    private readonly IDeterministicExtractor _deterministicExtractor;
    private readonly ILogger<PropertyResolver> _logger;
    private const double FuzzyThreshold = 0.9;

    public PropertyResolver(
        IFuzzyMatcher fuzzyMatcher,
        IHeuristicExtractor heuristicExtractor,
        IDeterministicExtractor deterministicExtractor,
        ILogger<PropertyResolver> logger)
    {
        _fuzzyMatcher = fuzzyMatcher;
        _heuristicExtractor = heuristicExtractor;
        _deterministicExtractor = deterministicExtractor;
        _logger = logger;
    }

    public async Task<string?> ResolveAsync(PropertyResolutionContext context, CancellationToken ct = default)
    {
        // Step 1: Exact memory match
        var exactResult = TryExactMatch(context);
        if (exactResult != null)
        {
            _logger.LogDebug("Property {Property} resolved via exact match: {Value}", context.PropertyName, exactResult);
            return exactResult;
        }

        // Step 2: Fuzzy match (threshold 0.9)
        var fuzzyResult = TryFuzzyMatch(context);
        if (fuzzyResult != null)
        {
            _logger.LogDebug("Property {Property} resolved via fuzzy match: {Value}", context.PropertyName, fuzzyResult);
            return fuzzyResult;
        }

        // Step 3: Heuristic extraction
        var heuristicResult = _heuristicExtractor.Extract(context);
        if (heuristicResult != null)
        {
            _logger.LogDebug("Property {Property} resolved via heuristic: {Value}", context.PropertyName, heuristicResult);
            return heuristicResult;
        }

        // Step 4: Deterministic rules
        var deterministicResult = _deterministicExtractor.Extract(context);
        if (deterministicResult != null)
        {
            _logger.LogDebug("Property {Property} resolved via deterministic: {Value}", context.PropertyName, deterministicResult);
            return deterministicResult;
        }

        // Step 5: AI (if enabled) - placeholder for future integration
        if (context.AllowAi)
        {
            // TODO: Integrate with IAiInferenceService
            _logger.LogDebug("Property {Property} would use AI (not implemented)", context.PropertyName);
        }

        // Step 6: User approval required
        if (context.RequireUserApproval)
        {
            _logger.LogInformation("Property {Property} requires user approval for product: {ProductName}",
                context.PropertyName, context.ProductName);
            // TODO: Create approval request
        }

        return null;
    }

    private string? TryExactMatch(PropertyResolutionContext context)
    {
        if (context.MemoryDictionary.TryGetValue(context.ProductName, out var value))
            return value;

        // Try with normalized key
        var normalizedKey = TextNormalizer.ToSearchForm(context.ProductName);
        foreach (var kvp in context.MemoryDictionary)
        {
            if (TextNormalizer.ToSearchForm(kvp.Key) == normalizedKey)
                return kvp.Value;
        }

        return null;
    }

    private string? TryFuzzyMatch(PropertyResolutionContext context)
    {
        if (context.MemoryDictionary.Count == 0)
            return null;

        var match = _fuzzyMatcher.FindBestMatchInDictionary(
            context.ProductName,
            (IDictionary<string, string>)context.MemoryDictionary,
            FuzzyThreshold);

        return match?.Value;
    }
}
