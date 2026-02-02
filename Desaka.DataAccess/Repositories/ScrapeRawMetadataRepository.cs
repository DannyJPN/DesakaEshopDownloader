using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ScrapeRawMetadataRepository : EfRepository<ScrapeRawMetadata>, IScrapeRawMetadataRepository
{
    public ScrapeRawMetadataRepository(DesakaDbContext db) : base(db)
    {
    }
}

