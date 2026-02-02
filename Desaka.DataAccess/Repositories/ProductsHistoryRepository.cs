using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class ProductsHistoryRepository : EfRepository<ProductsHistory>, IProductsHistoryRepository
{
    public ProductsHistoryRepository(DesakaDbContext db) : base(db)
    {
    }
}

