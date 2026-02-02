using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ScrapeRunRepository : EfRepository<ScrapeRun>, IScrapeRunRepository
{
    public ScrapeRunRepository(DesakaDbContext db) : base(db)
    {
    }
}

