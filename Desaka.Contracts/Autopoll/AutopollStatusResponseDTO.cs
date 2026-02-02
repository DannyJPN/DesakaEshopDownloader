using Desaka.Contracts.Common;

namespace Desaka.Contracts.Autopoll;

public sealed record AutopollStatusResponseDTO(long RunId, JobStatus Status, int? Progress, DateTime StartedAt, DateTime? UpdatedAt, string? Message);
