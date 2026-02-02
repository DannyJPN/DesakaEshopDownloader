using Desaka.AI.Application;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Desaka.AI.Infrastructure;

public sealed class PricingUpdater : BackgroundService
{
    private readonly IAiService _aiService;
    private readonly ILogger<PricingUpdater> _logger;

    public PricingUpdater(IAiService aiService, ILogger<PricingUpdater> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _aiService.UpdatePricingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI pricing update failed.");
            }

            await Task.Delay(TimeSpan.FromDays(30), stoppingToken);
        }
    }
}
