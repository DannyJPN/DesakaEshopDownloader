using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryVariantNameMemoryRepository : EfRepository<MemoryVariantNameMemory>, IMemoryVariantNameMemoryRepository
{
    public MemoryVariantNameMemoryRepository(DesakaDbContext db) : base(db)
    {
    }
}

