using System.Text.Json;
using Desaka.AI.Application;
using Microsoft.Extensions.Logging;

namespace Desaka.AI.Infrastructure;

/// <summary>
/// High-level AI inference service that manages providers and tracks costs.
/// </summary>
public sealed class AiInferenceService : IAiInferenceService
{
    private readonly IReadOnlyList<IAiProvider> _providers;
    private readonly IAiProvider _defaultProvider;
    private readonly AiCostTracker _costTracker;
    private readonly ILogger<AiInferenceService> _logger;

    public AiInferenceService(
        IEnumerable<IAiProvider> providers,
        AiCostTracker costTracker,
        ILogger<AiInferenceService> logger)
    {
        _providers = providers.ToList();
        _defaultProvider = _providers.FirstOrDefault() ?? throw new InvalidOperationException("No AI providers configured");
        _costTracker = costTracker;
        _logger = logger;
    }

    public IReadOnlyList<string> GetAvailableProviders() => _providers.Select(p => p.ProviderId).ToList();

    public async Task<AiInferenceResponse> InferAsync(string prompt, string? systemPrompt = null, CancellationToken ct = default)
    {
        var request = new AiInferenceRequest(systemPrompt ?? "", prompt);
        var response = await _defaultProvider.InferAsync(request, ct);

        if (response.Success)
            _costTracker.TrackUsage(_defaultProvider.ProviderId, response.Model, response.PromptTokens, response.CompletionTokens);

        return response;
    }

    public async Task<T?> InferJsonAsync<T>(string prompt, string? systemPrompt = null, CancellationToken ct = default) where T : class
    {
        var request = new AiInferenceRequest(systemPrompt ?? "", prompt, ExpectJson: true);
        var response = await _defaultProvider.InferAsync(request, ct);

        if (response.Success)
            _costTracker.TrackUsage(_defaultProvider.ProviderId, response.Model, response.PromptTokens, response.CompletionTokens);

        if (!response.Success || string.IsNullOrEmpty(response.Content))
            return null;

        try
        {
            var content = response.Content;
            // Clean markdown code blocks
            if (content.StartsWith("```json")) content = content[7..];
            else if (content.StartsWith("```")) content = content[3..];
            if (content.EndsWith("```")) content = content[..^3];

            return JsonSerializer.Deserialize<T>(content.Trim());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON response");
            return null;
        }
    }
}
