using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Desaka.AI.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Desaka.AI.Infrastructure.Providers;

/// <summary>
/// Anthropic Claude API provider implementation.
/// </summary>
public sealed class AnthropicProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AnthropicProvider> _logger;
    private readonly string _defaultModel;
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    public string ProviderId => "anthropic";
    public bool SupportsImages => true;
    public bool SupportsBatch => false;

    public AnthropicProvider(HttpClient httpClient, IConfiguration config, ILogger<AnthropicProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _defaultModel = config["AI:Anthropic:DefaultModel"] ?? "claude-3-5-sonnet-20241022";

        var apiKey = config["AI:Anthropic:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }
    }

    public async Task<AiInferenceResponse> InferAsync(AiInferenceRequest request, CancellationToken ct = default)
    {
        try
        {
            var model = request.Model ?? _defaultModel;

            var userContent = request.UserPrompt;
            if (request.ExpectJson)
                userContent += "\n\nRespond with valid JSON only, no additional text.";

            var payload = new
            {
                model,
                max_tokens = request.MaxTokens ?? 4096,
                system = request.SystemPrompt ?? "",
                messages = new[] { new { role = "user", content = userContent } }
            };

            var response = await _httpClient.PostAsJsonAsync(ApiUrl, payload, ct);
            var json = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Anthropic API error: {Status} - {Body}", response.StatusCode, json);
                return new AiInferenceResponse("", 0, 0, model, false, $"API error: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<AnthropicResponse>(json);
            if (result?.Content == null || result.Content.Count == 0)
                return new AiInferenceResponse("", 0, 0, model, false, "No response from API");

            var content = result.Content[0].Text ?? "";
            return new AiInferenceResponse(
                content.Trim(),
                result.Usage?.InputTokens ?? 0,
                result.Usage?.OutputTokens ?? 0,
                model,
                true
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Anthropic inference failed");
            return new AiInferenceResponse("", 0, 0, request.Model ?? _defaultModel, false, ex.Message);
        }
    }

    private class AnthropicResponse
    {
        [JsonPropertyName("content")] public List<ContentBlock>? Content { get; set; }
        [JsonPropertyName("usage")] public Usage? Usage { get; set; }
    }

    private class ContentBlock
    {
        [JsonPropertyName("text")] public string? Text { get; set; }
    }

    private class Usage
    {
        [JsonPropertyName("input_tokens")] public int InputTokens { get; set; }
        [JsonPropertyName("output_tokens")] public int OutputTokens { get; set; }
    }
}
