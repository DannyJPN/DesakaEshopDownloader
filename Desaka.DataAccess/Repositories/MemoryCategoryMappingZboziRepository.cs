using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryCategoryMappingZboziRepository : EfRepository<MemoryCategoryMappingZbozi>, IMemoryCategoryMappingZboziRepository
{
    public MemoryCategoryMappingZboziRepository(DesakaDbContext db) : base(db)
    {
    }
}

