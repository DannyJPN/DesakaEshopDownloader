using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryCategorySubCodeListRepository : EfRepository<MemoryCategorySubCodeList>, IMemoryCategorySubCodeListRepository
{
    public MemoryCategorySubCodeListRepository(DesakaDbContext db) : base(db)
    {
    }
}

