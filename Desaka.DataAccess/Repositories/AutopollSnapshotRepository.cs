using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class AutopollSnapshotRepository : EfRepository<AutopollSnapshot>, IAutopollSnapshotRepository
{
    public AutopollSnapshotRepository(DesakaDbContext db) : base(db)
    {
    }
}

