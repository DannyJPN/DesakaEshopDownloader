namespace Desaka.Validation.Application;

public interface IValidationService
{
    Task<ValidationResult> ValidateUrlAsync(string url, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateApiKeyAsync(string baseUrl, string apiKey, CancellationToken cancellationToken = default);
}

public sealed record ValidationResult(bool IsValid, string? Message = null);
