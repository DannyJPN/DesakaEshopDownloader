using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryCategoryIdListRepository : EfRepository<MemoryCategoryIdList>, IMemoryCategoryIdListRepository
{
    public MemoryCategoryIdListRepository(DesakaDbContext db) : base(db)
    {
    }
}

