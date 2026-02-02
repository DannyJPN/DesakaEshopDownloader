using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ConfigLanguageRepository : EfRepository<ConfigLanguage>, IConfigLanguageRepository
{
    public ConfigLanguageRepository(DesakaDbContext db) : base(db)
    {
    }
}

