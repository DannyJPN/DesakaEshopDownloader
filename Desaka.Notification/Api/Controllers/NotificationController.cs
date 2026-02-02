using System.Text.Json;
using Desaka.Contracts.Common;
using Desaka.Contracts.EventBus;
using Desaka.Contracts.Notify;
using Desaka.EventBus;
using Desaka.Notification.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Desaka.Notification.Api.Controllers;

[ApiController]
[Route("api/v1/notify")]
public sealed class NotificationController : ControllerBase
{
    private readonly IEventBus _eventBus;
    private readonly NotificationStream _stream;

    public NotificationController(IEventBus eventBus, NotificationStream stream)
    {
        _eventBus = eventBus;
        _stream = stream;
    }

    [HttpPost("push")]
    public async Task<IActionResult> Push([FromBody] NotificationRequestDTO request)
    {
        var envelope = new EventEnvelope(
            request.EventType,
            "Notification",
            DateTime.UtcNow,
            request.CorrelationId,
            JsonSerializer.Serialize(request));

        await _eventBus.PublishAsync(envelope, HttpContext.RequestAborted);
        return Ok();
    }

    [HttpGet("status")]
    public ActionResult<NotificationStatusResponseDTO> Status() => Ok(new NotificationStatusResponseDTO(true, Array.Empty<string>()));

    [HttpGet("stream")]
    public async Task Stream(CancellationToken cancellationToken)
    {
        Response.Headers["Content-Type"] = "text/event-stream";

        await foreach (var notification in _stream.ReadAllAsync(cancellationToken))
        {
            var json = JsonSerializer.Serialize(notification);
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}

