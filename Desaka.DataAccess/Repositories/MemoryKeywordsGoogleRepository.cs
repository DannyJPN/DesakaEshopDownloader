using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryKeywordsGoogleRepository : EfRepository<MemoryKeywordsGoogle>, IMemoryKeywordsGoogleRepository
{
    public MemoryKeywordsGoogleRepository(DesakaDbContext db) : base(db)
    {
    }
}

