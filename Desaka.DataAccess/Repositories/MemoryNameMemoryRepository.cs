using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryNameMemoryRepository : EfRepository<MemoryNameMemory>, IMemoryNameMemoryRepository
{
    public MemoryNameMemoryRepository(DesakaDbContext db) : base(db)
    {
    }
}

