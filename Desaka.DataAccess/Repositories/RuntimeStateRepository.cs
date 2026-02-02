using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class RuntimeStateRepository : EfRepository<RuntimeState>, IRuntimeStateRepository
{
    public RuntimeStateRepository(DesakaDbContext db) : base(db)
    {
    }
}

