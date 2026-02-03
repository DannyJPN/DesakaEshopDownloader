using Microsoft.AspNetCore.SignalR;

namespace Desaka.Notification.Infrastructure;

/// <summary>
/// SignalR hub for real-time notifications.
/// </summary>
public sealed class NotificationHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SubscribeToEshop(int eshopId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"eshop-{eshopId}");
    }

    public async Task UnsubscribeFromEshop(int eshopId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"eshop-{eshopId}");
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "all");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "all");
        await base.OnDisconnectedAsync(exception);
    }
}
