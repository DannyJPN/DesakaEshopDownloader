using Desaka.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Desaka.DataAccess.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly DesakaDbContext _db;
    private readonly DbSet<T> _set;

    public EfRepository(DesakaDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        => _set.FindAsync(new[] { id }, cancellationToken).AsTask();

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
        => await _set.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _set.AddAsync(entity, cancellationToken);

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _set.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _set.Remove(entity);
        return Task.CompletedTask;
    }
}
