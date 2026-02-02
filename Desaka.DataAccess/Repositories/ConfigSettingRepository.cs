using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ConfigSettingRepository : EfRepository<ConfigSetting>, IConfigSettingRepository
{
    public ConfigSettingRepository(DesakaDbContext db) : base(db)
    {
    }
}

