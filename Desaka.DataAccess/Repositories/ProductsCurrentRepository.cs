using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ProductsCurrentRepository : EfRepository<ProductsCurrent>, IProductsCurrentRepository
{
    public ProductsCurrentRepository(DesakaDbContext db) : base(db)
    {
    }
}

