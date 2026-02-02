namespace Desaka.Contracts.Common;

public sealed record PageRequest(int Page = 1, int PageSize = 50);
public sealed record PageResult<T>(IReadOnlyList<T> Items, int TotalCount);
