namespace Desaka.AI.Application;

/// <summary>
/// High-level inference service that manages providers and routing.
/// </summary>
public interface IAiInferenceService
{
    Task<AiInferenceResponse> InferAsync(string prompt, string? systemPrompt = null, CancellationToken ct = default);
    Task<T?> InferJsonAsync<T>(string prompt, string? systemPrompt = null, CancellationToken ct = default) where T : class;
    IReadOnlyList<string> GetAvailableProviders();
}
