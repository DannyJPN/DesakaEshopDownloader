using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryVariantValueMemoryRepository : EfRepository<MemoryVariantValueMemory>, IMemoryVariantValueMemoryRepository
{
    public MemoryVariantValueMemoryRepository(DesakaDbContext db) : base(db)
    {
    }
}

