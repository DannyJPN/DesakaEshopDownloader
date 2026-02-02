using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryDescMemoryRepository : EfRepository<MemoryDescMemory>, IMemoryDescMemoryRepository
{
    public MemoryDescMemoryRepository(DesakaDbContext db) : base(db)
    {
    }
}

