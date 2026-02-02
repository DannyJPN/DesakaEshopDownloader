using Desaka.Contracts.Common;

namespace Desaka.Contracts.Autopoll;

public sealed record AutopollStartResponseDTO(long RunId, JobStatus Status);
