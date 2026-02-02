namespace Desaka.Contracts.Unifier;

public sealed record ApprovalItemDTO(long Id, string ProductCode, string Url, string? ImageUrl, string PropertyName, string? CurrentValue, string? SuggestedValue, string Language);
