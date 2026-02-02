using Desaka.Contracts.Memory;

namespace Desaka.Memory.Application;

public interface IMemoryService
{
    Task<IReadOnlyList<MemoryEntryDTO>> SearchAsync(MemorySearchRequestDTO request, CancellationToken cancellationToken = default);
    Task<MemoryEntryDTO> CreateAsync(string memoryType, MemoryEntryDTO entry, CancellationToken cancellationToken = default);
    Task<MemoryEntryDTO?> UpdateAsync(string memoryType, MemoryEntryDTO entry, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string memoryType, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemoryEntryDTO>> ExportAsync(string memoryType, string? languageCode, CancellationToken cancellationToken = default);
    Task<MemoryImportResponseDTO> ImportAsync(string memoryType, Stream fileStream, string? languageCode, CancellationToken cancellationToken = default);
}

