using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ConfigAiProviderCatalogRepository : EfRepository<ConfigAiProviderCatalog>, IConfigAiProviderCatalogRepository
{
    public ConfigAiProviderCatalogRepository(DesakaDbContext db) : base(db)
    {
    }
}

