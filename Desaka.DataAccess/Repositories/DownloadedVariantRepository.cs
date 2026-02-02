using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class DownloadedVariantRepository : EfRepository<DownloadedVariant>, IDownloadedVariantRepository
{
    public DownloadedVariantRepository(DesakaDbContext db) : base(db)
    {
    }
}

