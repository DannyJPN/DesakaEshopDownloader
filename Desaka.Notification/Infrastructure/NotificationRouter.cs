using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Desaka.Notification.Infrastructure;

/// <summary>
/// Routes notifications to appropriate SignalR clients.
/// </summary>
public interface INotificationRouter
{
    Task SendToAllAsync(string eventType, object payload, CancellationToken ct = default);
    Task SendToGroupAsync(string group, string eventType, object payload, CancellationToken ct = default);
    Task SendToUserAsync(string userId, string eventType, object payload, CancellationToken ct = default);
    Task SendToEshopAsync(int eshopId, string eventType, object payload, CancellationToken ct = default);
}

public sealed class NotificationRouter : INotificationRouter
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationRouter> _logger;

    public NotificationRouter(IHubContext<NotificationHub> hubContext, ILogger<NotificationRouter> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToAllAsync(string eventType, object payload, CancellationToken ct = default)
    {
        _logger.LogDebug("Sending notification to all: {EventType}", eventType);
        await _hubContext.Clients.Group("all").SendAsync(eventType, payload, ct);
    }

    public async Task SendToGroupAsync(string group, string eventType, object payload, CancellationToken ct = default)
    {
        _logger.LogDebug("Sending notification to group {Group}: {EventType}", group, eventType);
        await _hubContext.Clients.Group(group).SendAsync(eventType, payload, ct);
    }

    public async Task SendToUserAsync(string userId, string eventType, object payload, CancellationToken ct = default)
    {
        _logger.LogDebug("Sending notification to user {UserId}: {EventType}", userId, eventType);
        await _hubContext.Clients.User(userId).SendAsync(eventType, payload, ct);
    }

    public async Task SendToEshopAsync(int eshopId, string eventType, object payload, CancellationToken ct = default)
    {
        _logger.LogDebug("Sending notification to eshop {EshopId}: {EventType}", eshopId, eventType);
        await _hubContext.Clients.Group($"eshop-{eshopId}").SendAsync(eventType, payload, ct);
    }
}
