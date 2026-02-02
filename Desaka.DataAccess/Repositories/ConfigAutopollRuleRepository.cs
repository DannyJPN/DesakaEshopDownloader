using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ConfigAutopollRuleRepository : EfRepository<ConfigAutopollRule>, IConfigAutopollRuleRepository
{
    public ConfigAutopollRuleRepository(DesakaDbContext db) : base(db)
    {
    }
}

