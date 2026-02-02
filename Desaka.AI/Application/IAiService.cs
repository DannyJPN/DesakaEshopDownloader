using Desaka.Contracts.Config;

namespace Desaka.AI.Application;

public interface IAiService
{
    Task<IReadOnlyList<AiPricingDto>> GetPricingAsync(CancellationToken cancellationToken = default);
    Task UpdatePricingAsync(CancellationToken cancellationToken = default);
}

public sealed record AiPricingDto(int ProviderId, string ModelName, decimal PriceInputPer1K, decimal PriceOutputPer1K, string Currency, DateTime? LastUpdatedAt);
