using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class AutopollRunRepository : EfRepository<AutopollRun>, IAutopollRunRepository
{
    public AutopollRunRepository(DesakaDbContext db) : base(db)
    {
    }
}

