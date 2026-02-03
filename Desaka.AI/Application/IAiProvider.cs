namespace Desaka.AI.Application;

/// <summary>
/// Interface for AI inference providers (OpenAI, Anthropic, etc.)
/// </summary>
public interface IAiProvider
{
    string ProviderId { get; }
    bool SupportsImages { get; }
    bool SupportsBatch { get; }
    Task<AiInferenceResponse> InferAsync(AiInferenceRequest request, CancellationToken ct = default);
}

public record AiInferenceRequest(
    string SystemPrompt,
    string UserPrompt,
    string? Model = null,
    float Temperature = 0.4f,
    int? MaxTokens = null,
    bool ExpectJson = false
);

public record AiInferenceResponse(
    string Content,
    int PromptTokens,
    int CompletionTokens,
    string Model,
    bool Success,
    string? Error = null
);
