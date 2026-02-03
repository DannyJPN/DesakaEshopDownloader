using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Desaka.AI.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Desaka.AI.Infrastructure.Providers;

/// <summary>
/// OpenAI API provider implementation.
/// </summary>
public sealed class OpenAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiProvider> _logger;
    private readonly string _defaultModel;
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    public string ProviderId => "openai";
    public bool SupportsImages => true;
    public bool SupportsBatch => true;

    public OpenAiProvider(HttpClient httpClient, IConfiguration config, ILogger<OpenAiProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _defaultModel = config["AI:OpenAI:DefaultModel"] ?? "gpt-4o-mini";

        var apiKey = config["AI:OpenAI:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        }
    }

    public async Task<AiInferenceResponse> InferAsync(AiInferenceRequest request, CancellationToken ct = default)
    {
        try
        {
            var model = request.Model ?? _defaultModel;
            var messages = new List<object>();

            if (!string.IsNullOrEmpty(request.SystemPrompt))
                messages.Add(new { role = "system", content = request.SystemPrompt });

            var userContent = request.UserPrompt;
            if (request.ExpectJson)
                userContent += "\n\nRespond with valid JSON only, no additional text.";

            messages.Add(new { role = "user", content = userContent });

            var payload = new
            {
                model,
                messages,
                temperature = request.Temperature,
                max_tokens = request.MaxTokens ?? 4096
            };

            var response = await _httpClient.PostAsJsonAsync(ApiUrl, payload, ct);
            var json = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API error: {Status} - {Body}", response.StatusCode, json);
                return new AiInferenceResponse("", 0, 0, model, false, $"API error: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<OpenAiResponse>(json);
            if (result?.Choices == null || result.Choices.Count == 0)
                return new AiInferenceResponse("", 0, 0, model, false, "No response from API");

            var content = result.Choices[0].Message?.Content ?? "";
            return new AiInferenceResponse(
                content.Trim(),
                result.Usage?.PromptTokens ?? 0,
                result.Usage?.CompletionTokens ?? 0,
                model,
                true
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI inference failed");
            return new AiInferenceResponse("", 0, 0, request.Model ?? _defaultModel, false, ex.Message);
        }
    }

    private class OpenAiResponse
    {
        [JsonPropertyName("choices")] public List<Choice>? Choices { get; set; }
        [JsonPropertyName("usage")] public Usage? Usage { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("message")] public Message? Message { get; set; }
    }

    private class Message
    {
        [JsonPropertyName("content")] public string? Content { get; set; }
    }

    private class Usage
    {
        [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; set; }
        [JsonPropertyName("completion_tokens")] public int CompletionTokens { get; set; }
    }
}
