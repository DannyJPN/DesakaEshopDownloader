using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ConfigEshopRepository : EfRepository<ConfigEshop>, IConfigEshopRepository
{
    public ConfigEshopRepository(DesakaDbContext db) : base(db)
    {
    }
}

