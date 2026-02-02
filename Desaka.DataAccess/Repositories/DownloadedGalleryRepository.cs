using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class DownloadedGalleryRepository : EfRepository<DownloadedGallery>, IDownloadedGalleryRepository
{
    public DownloadedGalleryRepository(DesakaDbContext db) : base(db)
    {
    }
}

