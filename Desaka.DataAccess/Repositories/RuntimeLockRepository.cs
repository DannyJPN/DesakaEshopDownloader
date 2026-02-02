using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class RuntimeLockRepository : EfRepository<RuntimeLock>, IRuntimeLockRepository
{
    public RuntimeLockRepository(DesakaDbContext db) : base(db)
    {
    }
}

