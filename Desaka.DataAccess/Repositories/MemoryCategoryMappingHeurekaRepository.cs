using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryCategoryMappingHeurekaRepository : EfRepository<MemoryCategoryMappingHeureka>, IMemoryCategoryMappingHeurekaRepository
{
    public MemoryCategoryMappingHeurekaRepository(DesakaDbContext db) : base(db)
    {
    }
}

