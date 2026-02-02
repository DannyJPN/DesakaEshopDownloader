using Desaka.Validation.Application;

namespace Desaka.Validation.Infrastructure;

public sealed class ValidationService : IValidationService
{
    private readonly HttpClient _httpClient;

    public ValidationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ValidationResult> ValidateUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return new ValidationResult(false, "Invalid URL.");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, uri);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode
                ? new ValidationResult(true)
                : new ValidationResult(false, $"Status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return new ValidationResult(false, ex.Message);
        }
    }

    public async Task<ValidationResult> ValidateApiKeyAsync(string baseUrl, string apiKey, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            return new ValidationResult(false, "Invalid base URL.");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri, "/health"));
            request.Headers.Add("X-Api-Key", apiKey);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode
                ? new ValidationResult(true)
                : new ValidationResult(false, $"Status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return new ValidationResult(false, ex.Message);
        }
    }
}
