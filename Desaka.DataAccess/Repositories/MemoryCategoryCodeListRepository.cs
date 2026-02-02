using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryCategoryCodeListRepository : EfRepository<MemoryCategoryCodeList>, IMemoryCategoryCodeListRepository
{
    public MemoryCategoryCodeListRepository(DesakaDbContext db) : base(db)
    {
    }
}

