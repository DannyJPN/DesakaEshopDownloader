namespace Desaka.Contracts.Memory;

public sealed record MemorySearchRequestDTO(string MemoryType, string Query, string? LanguageCode, string Mode);
