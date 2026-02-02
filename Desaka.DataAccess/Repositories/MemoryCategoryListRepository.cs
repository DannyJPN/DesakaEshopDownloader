using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryCategoryListRepository : EfRepository<MemoryCategoryList>, IMemoryCategoryListRepository
{
    public MemoryCategoryListRepository(DesakaDbContext db) : base(db)
    {
    }
}

