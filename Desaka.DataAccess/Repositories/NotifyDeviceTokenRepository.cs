using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class NotifyDeviceTokenRepository : EfRepository<NotifyDeviceToken>, INotifyDeviceTokenRepository
{
    public NotifyDeviceTokenRepository(DesakaDbContext db) : base(db)
    {
    }
}

