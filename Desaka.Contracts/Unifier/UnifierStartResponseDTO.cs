using Desaka.Contracts.Common;

namespace Desaka.Contracts.Unifier;

public sealed record UnifierStartResponseDTO(long RunId, JobStatus Status);
