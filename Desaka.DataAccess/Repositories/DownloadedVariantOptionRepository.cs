using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class DownloadedVariantOptionRepository : EfRepository<DownloadedVariantOption>, IDownloadedVariantOptionRepository
{
    public DownloadedVariantOptionRepository(DesakaDbContext db) : base(db)
    {
    }
}

