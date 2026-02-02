using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryProductBrandMemoryRepository : EfRepository<MemoryProductBrandMemory>, IMemoryProductBrandMemoryRepository
{
    public MemoryProductBrandMemoryRepository(DesakaDbContext db) : base(db)
    {
    }
}

