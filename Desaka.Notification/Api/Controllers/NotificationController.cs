using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;

namespace Desaka.Notification.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private static readonly Channel<NotificationEvent> _channel = Channel.CreateUnbounded<NotificationEvent>();

    /// <summary>
    /// Server-Sent Events endpoint for streaming notifications.
    /// </summary>
    [HttpGet("stream")]
    public async Task StreamNotifications(CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        try
        {
            await foreach (var notification in _channel.Reader.ReadAllAsync(ct))
            {
                await Response.WriteAsync($"event: {notification.Type}\n", ct);
                await Response.WriteAsync($"data: {JsonSerializer.Serialize(notification.Data)}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected
        }
    }

    /// <summary>
    /// Broadcast a notification to all SSE clients.
    /// </summary>
    [HttpPost("broadcast")]
    public async Task<IActionResult> Broadcast([FromBody] NotificationEvent notification, CancellationToken ct)
    {
        await _channel.Writer.WriteAsync(notification, ct);
        return Ok();
    }

    public record NotificationEvent(string Type, object Data);
}
