using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ConfigAiProviderAssignmentRepository : EfRepository<ConfigAiProviderAssignment>, IConfigAiProviderAssignmentRepository
{
    public ConfigAiProviderAssignmentRepository(DesakaDbContext db) : base(db)
    {
    }
}

