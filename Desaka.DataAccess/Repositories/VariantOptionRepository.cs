using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class VariantOptionRepository : EfRepository<VariantOption>, IVariantOptionRepository
{
    public VariantOptionRepository(DesakaDbContext db) : base(db)
    {
    }
}

