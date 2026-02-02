using Desaka.Contracts.Common;

namespace Desaka.Contracts.Unifier;

public sealed record ApprovalActionRequestDTO(long ApprovalId, ApprovalAction Action, string? Value = null);
