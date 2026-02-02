using Desaka.Contracts.Common;

namespace Desaka.Contracts.Autopoll;

public sealed record AutopollStartRequestDTO(long? RuleId = null, int? EshopId = null, string? CorrelationId = null);
