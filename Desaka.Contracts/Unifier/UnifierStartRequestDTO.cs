using Desaka.Contracts.Common;

namespace Desaka.Contracts.Unifier;

public sealed record UnifierStartRequestDTO(bool RunWebscrapeFirst = false, string? CorrelationId = null);
