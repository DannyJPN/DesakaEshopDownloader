using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class MemoryWrongsRepository : EfRepository<MemoryWrongs>, IMemoryWrongsRepository
{
    public MemoryWrongsRepository(DesakaDbContext db) : base(db)
    {
    }
}

