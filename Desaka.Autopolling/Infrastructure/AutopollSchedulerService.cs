using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Desaka.Autopolling.Infrastructure;

/// <summary>
/// Background service that schedules and triggers autopoll operations.
/// </summary>
public sealed class AutopollSchedulerService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IScheduleCalculator _calculator;
    private readonly ILogger<AutopollSchedulerService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
    private readonly Dictionary<int, DateTime> _lastRuns = new();

    public AutopollSchedulerService(
        IServiceProvider services,
        IScheduleCalculator calculator,
        ILogger<AutopollSchedulerService> logger)
    {
        _services = services;
        _calculator = calculator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Autopoll scheduler started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndTriggerAutopolls(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in autopoll scheduler");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Autopoll scheduler stopped");
    }

    private async Task CheckAndTriggerAutopolls(CancellationToken ct)
    {
        using var scope = _services.CreateScope();

        // Get all configured eshops with autopoll enabled
        var configs = await GetAutopollConfigs(scope, ct);

        foreach (var (eshopId, config) in configs)
        {
            var lastRun = _lastRuns.GetValueOrDefault(eshopId, DateTime.MinValue);

            if (_calculator.ShouldRunNow(config, lastRun))
            {
                _logger.LogInformation("Triggering autopoll for eshop {EshopId}", eshopId);

                try
                {
                    await TriggerAutopoll(scope, eshopId, ct);
                    _lastRuns[eshopId] = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to trigger autopoll for eshop {EshopId}", eshopId);
                }
            }
        }
    }

    private Task<Dictionary<int, ScheduleConfig>> GetAutopollConfigs(IServiceScope scope, CancellationToken ct)
    {
        // TODO: Load from database or configuration
        // For now, return empty dictionary
        return Task.FromResult(new Dictionary<int, ScheduleConfig>());
    }

    private Task TriggerAutopoll(IServiceScope scope, int eshopId, CancellationToken ct)
    {
        // TODO: Resolve IAutopollService and trigger
        _logger.LogDebug("Would trigger autopoll for eshop {EshopId}", eshopId);
        return Task.CompletedTask;
    }
}
