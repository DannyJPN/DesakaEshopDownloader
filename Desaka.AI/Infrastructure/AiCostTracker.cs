using System.Collections.Concurrent;

namespace Desaka.AI.Infrastructure;

/// <summary>
/// Tracks AI API usage and calculates costs.
/// </summary>
public sealed class AiCostTracker
{
    private readonly ConcurrentDictionary<string, UsageStats> _usageByModel = new();
    private readonly Dictionary<string, (decimal Input, decimal Output)> _pricing = new()
    {
        // OpenAI pricing per 1K tokens
        ["gpt-4o"] = (0.005m, 0.015m),
        ["gpt-4o-mini"] = (0.00015m, 0.0006m),
        ["gpt-4-turbo"] = (0.01m, 0.03m),
        // Anthropic pricing per 1K tokens
        ["claude-3-5-sonnet-20241022"] = (0.003m, 0.015m),
        ["claude-3-haiku-20240307"] = (0.00025m, 0.00125m),
    };

    public void TrackUsage(string providerId, string model, int promptTokens, int completionTokens)
    {
        var key = $"{providerId}:{model}";
        _usageByModel.AddOrUpdate(key,
            _ => new UsageStats { PromptTokens = promptTokens, CompletionTokens = completionTokens, CallCount = 1 },
            (_, stats) =>
            {
                stats.PromptTokens += promptTokens;
                stats.CompletionTokens += completionTokens;
                stats.CallCount++;
                return stats;
            });
    }

    public decimal GetTotalCost()
    {
        decimal total = 0;
        foreach (var (key, stats) in _usageByModel)
        {
            var model = key.Split(':').LastOrDefault() ?? "";
            if (_pricing.TryGetValue(model, out var prices))
            {
                total += (stats.PromptTokens / 1000m) * prices.Input;
                total += (stats.CompletionTokens / 1000m) * prices.Output;
            }
        }
        return total;
    }

    public IReadOnlyDictionary<string, UsageStats> GetUsageStats() =>
        _usageByModel.ToDictionary(x => x.Key, x => x.Value);

    public void Reset() => _usageByModel.Clear();

    public class UsageStats
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int CallCount { get; set; }
    }
}
