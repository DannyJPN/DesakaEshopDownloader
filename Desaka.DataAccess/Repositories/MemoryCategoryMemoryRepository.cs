using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryCategoryMemoryRepository : EfRepository<MemoryCategoryMemory>, IMemoryCategoryMemoryRepository
{
    public MemoryCategoryMemoryRepository(DesakaDbContext db) : base(db)
    {
    }
}

