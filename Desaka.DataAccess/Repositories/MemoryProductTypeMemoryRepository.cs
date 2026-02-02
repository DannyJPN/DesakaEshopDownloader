using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryProductTypeMemoryRepository : EfRepository<MemoryProductTypeMemory>, IMemoryProductTypeMemoryRepository
{
    public MemoryProductTypeMemoryRepository(DesakaDbContext db) : base(db)
    {
    }
}

