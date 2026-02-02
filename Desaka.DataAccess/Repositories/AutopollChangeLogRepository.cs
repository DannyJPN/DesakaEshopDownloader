using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class AutopollChangeLogRepository : EfRepository<AutopollChangeLog>, IAutopollChangeLogRepository
{
    public AutopollChangeLogRepository(DesakaDbContext db) : base(db)
    {
    }
}

