namespace Desaka.Contracts.Autopoll;

public sealed record AutopollBatchCommitRequestDTO(DateTime? From = null, DateTime? To = null);
