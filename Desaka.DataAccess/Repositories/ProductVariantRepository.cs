using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ProductVariantRepository : EfRepository<ProductVariant>, IProductVariantRepository
{
    public ProductVariantRepository(DesakaDbContext db) : base(db)
    {
    }
}

