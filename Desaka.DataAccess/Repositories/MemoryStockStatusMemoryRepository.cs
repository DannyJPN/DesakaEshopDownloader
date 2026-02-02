using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryStockStatusMemoryRepository : EfRepository<MemoryStockStatusMemory>, IMemoryStockStatusMemoryRepository
{
    public MemoryStockStatusMemoryRepository(DesakaDbContext db) : base(db)
    {
    }
}

