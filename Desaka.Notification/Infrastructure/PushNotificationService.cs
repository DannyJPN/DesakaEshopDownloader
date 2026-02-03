using Microsoft.Extensions.Logging;

namespace Desaka.Notification.Infrastructure;

/// <summary>
/// Service for sending push notifications (future mobile/desktop integration).
/// </summary>
public interface IPushNotificationService
{
    Task SendAsync(string userId, string title, string message, CancellationToken ct = default);
    Task SendToAllAsync(string title, string message, CancellationToken ct = default);
}

public sealed class PushNotificationService : IPushNotificationService
{
    private readonly INotificationRouter _router;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(INotificationRouter router, ILogger<PushNotificationService> logger)
    {
        _router = router;
        _logger = logger;
    }

    public async Task SendAsync(string userId, string title, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("Push notification to {UserId}: {Title}", userId, title);
        await _router.SendToUserAsync(userId, "push", new { title, message, timestamp = DateTime.UtcNow }, ct);
    }

    public async Task SendToAllAsync(string title, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("Push notification to all: {Title}", title);
        await _router.SendToAllAsync("push", new { title, message, timestamp = DateTime.UtcNow }, ct);
    }
}
