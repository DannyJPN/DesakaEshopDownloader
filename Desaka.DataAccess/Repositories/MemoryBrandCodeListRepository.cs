using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryBrandCodeListRepository : EfRepository<MemoryBrandCodeList>, IMemoryBrandCodeListRepository
{
    public MemoryBrandCodeListRepository(DesakaDbContext db) : base(db)
    {
    }
}

