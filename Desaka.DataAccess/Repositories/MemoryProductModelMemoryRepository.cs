using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryProductModelMemoryRepository : EfRepository<MemoryProductModelMemory>, IMemoryProductModelMemoryRepository
{
    public MemoryProductModelMemoryRepository(DesakaDbContext db) : base(db)
    {
    }
}

