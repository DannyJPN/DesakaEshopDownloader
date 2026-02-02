namespace Desaka.Contracts.Memory;

public sealed record MemoryEntryDTO(int Id, string Key, string Value, string Source, DateTime CreatedAt, DateTime UpdatedAt, string? LanguageCode = null);
