using Desaka.Contracts.Common;

namespace Desaka.Contracts.Unifier;

public sealed record UnifierStatusResponseDTO(long RunId, JobStatus Status, int? Progress, DateTime StartedAt, DateTime? UpdatedAt, string? Message);
