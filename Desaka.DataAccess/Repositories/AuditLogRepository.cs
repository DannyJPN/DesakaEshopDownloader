using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class AuditLogRepository : EfRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(DesakaDbContext db) : base(db)
    {
    }
}

