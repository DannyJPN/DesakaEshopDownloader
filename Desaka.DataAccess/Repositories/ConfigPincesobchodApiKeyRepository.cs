using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ConfigPincesobchodApiKeyRepository : EfRepository<ConfigPincesobchodApiKey>, IConfigPincesobchodApiKeyRepository
{
    public ConfigPincesobchodApiKeyRepository(DesakaDbContext db) : base(db)
    {
    }
}

