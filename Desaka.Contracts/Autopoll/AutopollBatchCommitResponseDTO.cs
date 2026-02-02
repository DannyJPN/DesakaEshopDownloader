using Desaka.Contracts.Common;

namespace Desaka.Contracts.Autopoll;

public sealed record AutopollBatchCommitResponseDTO(long BatchId, JobStatus Status);
