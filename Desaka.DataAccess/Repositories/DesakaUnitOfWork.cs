using Desaka.DataAccess.Abstractions;

namespace Desaka.DataAccess.Repositories;

public sealed class DesakaUnitOfWork : IUnitOfWork
{
    private readonly DesakaDbContext _db;

    public DesakaUnitOfWork(DesakaDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
