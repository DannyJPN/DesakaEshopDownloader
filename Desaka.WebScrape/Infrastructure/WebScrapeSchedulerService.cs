using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Desaka.WebScrape.Infrastructure;

/// <summary>
/// Background service that schedules and triggers web scraping operations.
/// </summary>
public sealed class WebScrapeSchedulerService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<WebScrapeSchedulerService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
    private readonly Dictionary<int, DateTime> _lastRuns = new();

    public WebScrapeSchedulerService(
        IServiceProvider services,
        ILogger<WebScrapeSchedulerService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WebScrape scheduler started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndTriggerScrapes(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in webscrape scheduler");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("WebScrape scheduler stopped");
    }

    private async Task CheckAndTriggerScrapes(CancellationToken ct)
    {
        using var scope = _services.CreateScope();

        // Get all pending scrape jobs
        var pendingJobs = await GetPendingScrapeJobs(scope, ct);

        foreach (var job in pendingJobs)
        {
            if (ShouldProcessJob(job))
            {
                _logger.LogInformation("Processing scrape job {JobId} for eshop {EshopId}", job.Id, job.EshopId);

                try
                {
                    await ProcessScrapeJob(scope, job, ct);
                    _lastRuns[job.EshopId] = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process scrape job {JobId}", job.Id);
                }
            }
        }
    }

    private Task<List<ScrapeJob>> GetPendingScrapeJobs(IServiceScope scope, CancellationToken ct)
    {
        // TODO: Load from database
        return Task.FromResult(new List<ScrapeJob>());
    }

    private bool ShouldProcessJob(ScrapeJob job)
    {
        // Check if enough time has passed since last run for this eshop
        var lastRun = _lastRuns.GetValueOrDefault(job.EshopId, DateTime.MinValue);
        return DateTime.UtcNow - lastRun >= TimeSpan.FromMinutes(5);
    }

    private Task ProcessScrapeJob(IServiceScope scope, ScrapeJob job, CancellationToken ct)
    {
        // TODO: Resolve scrape service and process
        _logger.LogDebug("Would process scrape job {JobId}", job.Id);
        return Task.CompletedTask;
    }

    private record ScrapeJob(int Id, int EshopId, string Url, DateTime ScheduledAt);
}
