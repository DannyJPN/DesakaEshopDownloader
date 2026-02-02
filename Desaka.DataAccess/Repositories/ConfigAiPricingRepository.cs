using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ConfigAiPricingRepository : EfRepository<ConfigAiPricing>, IConfigAiPricingRepository
{
    public ConfigAiPricingRepository(DesakaDbContext db) : base(db)
    {
    }
}

