using Desaka.Contracts.Unifier;

namespace Desaka.Unifier.Application;

public interface IUnifierService
{
    Task<UnifierStartResponseDTO> StartAsync(UnifierStartRequestDTO request, CancellationToken cancellationToken = default);
    Task<UnifierStatusResponseDTO?> GetStatusAsync(long runId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApprovalItemDTO>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
    Task<ApprovalActionResponseDTO> ApproveAsync(ApprovalActionRequestDTO request, CancellationToken cancellationToken = default);
}

