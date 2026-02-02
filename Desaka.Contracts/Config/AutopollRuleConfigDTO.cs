using Desaka.Contracts.Common;

namespace Desaka.Contracts.Config;

public sealed record AutopollRuleConfigDTO(int Id, string Name, bool IsEnabled, int EshopId, int IntervalValue, IntervalUnit IntervalUnit, TimeSpan? WindowStart, TimeSpan? WindowEnd, string FilterDefinition);
