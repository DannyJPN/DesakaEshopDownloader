using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryCategoryMappingGlamiRepository : EfRepository<MemoryCategoryMappingGlami>, IMemoryCategoryMappingGlamiRepository
{
    public MemoryCategoryMappingGlamiRepository(DesakaDbContext db) : base(db)
    {
    }
}

