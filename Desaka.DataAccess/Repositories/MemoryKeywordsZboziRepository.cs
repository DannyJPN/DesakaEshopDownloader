using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryKeywordsZboziRepository : EfRepository<MemoryKeywordsZbozi>, IMemoryKeywordsZboziRepository
{
    public MemoryKeywordsZboziRepository(DesakaDbContext db) : base(db)
    {
    }
}

