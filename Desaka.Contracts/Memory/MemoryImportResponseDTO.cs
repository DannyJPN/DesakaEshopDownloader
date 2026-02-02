namespace Desaka.Contracts.Memory;

public sealed record MemoryImportResponseDTO(int ImportedCount, IReadOnlyList<string> Warnings);
