using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryShortDescMemoryRepository : EfRepository<MemoryShortDescMemory>, IMemoryShortDescMemoryRepository
{
    public MemoryShortDescMemoryRepository(DesakaDbContext db) : base(db)
    {
    }
}

