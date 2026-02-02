using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryItemFilterRepository : EfRepository<MemoryItemFilter>, IMemoryItemFilterRepository
{
    public MemoryItemFilterRepository(DesakaDbContext db) : base(db)
    {
    }
}

