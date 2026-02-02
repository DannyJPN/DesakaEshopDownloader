using Desaka.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desaka.DataAccess;

public interface IRuntimeLockService
{
    Task<bool> TryAcquireAsync(string resourceKey, string ownerId, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task ReleaseAsync(string resourceKey, string ownerId, CancellationToken cancellationToken = default);
    Task HeartbeatAsync(string resourceKey, string ownerId, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task<bool> IsLockedAsync(string resourceKey, CancellationToken cancellationToken = default);
}

public sealed class RuntimeLockService : IRuntimeLockService
{
    private readonly DesakaDbContext _db;

    public RuntimeLockService(DesakaDbContext db)
    {
        _db = db;
    }

    public async Task<bool> TryAcquireAsync(string resourceKey, string ownerId, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var existing = await _db.RuntimeLocks.FirstOrDefaultAsync(x => x.ResourceKey == resourceKey, cancellationToken);
        if (existing != null && existing.ExpiresAt > now && existing.Status == "running")
        {
            return false;
        }

        if (existing == null)
        {
            existing = new RuntimeLock
            {
                ResourceKey = resourceKey,
                OwnerId = ownerId,
                StartedAt = now,
                LastHeartbeat = now,
                ExpiresAt = now.Add(ttl),
                Status = "running"
            };
            _db.RuntimeLocks.Add(existing);
        }
        else
        {
            existing.OwnerId = ownerId;
            existing.StartedAt = now;
            existing.LastHeartbeat = now;
            existing.ExpiresAt = now.Add(ttl);
            existing.Status = "running";
        }

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ReleaseAsync(string resourceKey, string ownerId, CancellationToken cancellationToken = default)
    {
        var existing = await _db.RuntimeLocks.FirstOrDefaultAsync(x => x.ResourceKey == resourceKey && x.OwnerId == ownerId, cancellationToken);
        if (existing == null)
        {
            return;
        }

        existing.Status = "released";
        existing.ExpiresAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task HeartbeatAsync(string resourceKey, string ownerId, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var existing = await _db.RuntimeLocks.FirstOrDefaultAsync(x => x.ResourceKey == resourceKey && x.OwnerId == ownerId, cancellationToken);
        if (existing == null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        existing.LastHeartbeat = now;
        existing.ExpiresAt = now.Add(ttl);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsLockedAsync(string resourceKey, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _db.RuntimeLocks.AsNoTracking().AnyAsync(x => x.ResourceKey == resourceKey && x.ExpiresAt > now && x.Status == "running", cancellationToken);
    }
}
