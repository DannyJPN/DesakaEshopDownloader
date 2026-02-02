using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class AiUsageLogRepository : EfRepository<AiUsageLog>, IAiUsageLogRepository
{
    public AiUsageLogRepository(DesakaDbContext db) : base(db)
    {
    }
}

