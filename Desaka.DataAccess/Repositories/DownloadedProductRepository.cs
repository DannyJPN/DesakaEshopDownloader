using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class DownloadedProductRepository : EfRepository<DownloadedProduct>, IDownloadedProductRepository
{
    public DownloadedProductRepository(DesakaDbContext db) : base(db)
    {
    }
}

