using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryCategoryMappingGoogleRepository : EfRepository<MemoryCategoryMappingGoogle>, IMemoryCategoryMappingGoogleRepository
{
    public MemoryCategoryMappingGoogleRepository(DesakaDbContext db) : base(db)
    {
    }
}

