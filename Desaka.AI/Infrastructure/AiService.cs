using Desaka.AI.Application;
using Desaka.DataAccess;
using Desaka.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desaka.AI.Infrastructure;

public sealed class AiService : IAiService
{
    private readonly DesakaDbContext _db;

    public AiService(DesakaDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AiPricingDto>> GetPricingAsync(CancellationToken cancellationToken = default)
    {
        var items = await _db.ConfigAiPricings.AsNoTracking().ToListAsync(cancellationToken);
        return items.Select(x => new AiPricingDto(x.ProviderId, x.ModelName, x.PriceInputPer1K, x.PriceOutputPer1K, x.Currency, x.LastUpdatedAt)).ToList();
    }

    public async Task UpdatePricingAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var items = await _db.ConfigAiPricings.ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            item.LastUpdatedAt = now;
        }

        if (items.Count == 0)
        {
            _db.ConfigAiPricings.Add(new ConfigAiPricing
            {
                ProviderId = 1,
                ModelName = "default",
                PriceInputPer1K = 0.0m,
                PriceOutputPer1K = 0.0m,
                Currency = "USD",
                LastUpdatedAt = now
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
