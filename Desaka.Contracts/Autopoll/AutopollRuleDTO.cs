using Desaka.Contracts.Common;

namespace Desaka.Contracts.Autopoll;

public sealed record AutopollRuleDTO(int Id, string Name, bool IsEnabled, int EshopId, int IntervalValue, IntervalUnit IntervalUnit, TimeSpan? WindowStart, TimeSpan? WindowEnd, string FilterDefinition, DateTime? LastRunAt, DateTime? NextRunAt, string? Note);
