using Desaka.Contracts.Autopoll;

namespace Desaka.Autopolling.Application;

public interface IAutopollService
{
    Task<AutopollStartResponseDTO> StartAsync(AutopollStartRequestDTO request, CancellationToken cancellationToken = default);
    Task<AutopollStatusResponseDTO?> GetStatusAsync(long? runId, int? ruleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AutopollRuleDTO>> GetRulesAsync(CancellationToken cancellationToken = default);
    Task<AutopollRuleDTO> UpsertRuleAsync(AutopollRuleDTO rule, CancellationToken cancellationToken = default);
    Task<bool> DeleteRuleAsync(int id, CancellationToken cancellationToken = default);
}

