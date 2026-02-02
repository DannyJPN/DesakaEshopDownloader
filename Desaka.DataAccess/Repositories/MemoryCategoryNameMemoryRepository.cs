using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryCategoryNameMemoryRepository : EfRepository<MemoryCategoryNameMemory>, IMemoryCategoryNameMemoryRepository
{
    public MemoryCategoryNameMemoryRepository(DesakaDbContext db) : base(db)
    {
    }
}

