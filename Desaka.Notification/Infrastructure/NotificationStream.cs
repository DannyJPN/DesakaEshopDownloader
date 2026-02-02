using System.Threading.Channels;
using Desaka.Contracts.EventBus;
using Desaka.Contracts.Notify;
using Desaka.EventBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Desaka.Notification.Infrastructure;

public sealed class NotificationStream
{
    private readonly Channel<NotificationRequestDTO> _channel = Channel.CreateUnbounded<NotificationRequestDTO>();

    public async Task PublishAsync(NotificationRequestDTO notification, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(notification, cancellationToken);
    }

    public IAsyncEnumerable<NotificationRequestDTO> ReadAllAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}

public sealed class NotificationEventSubscriber : BackgroundService
{
    private readonly IEventBus _eventBus;
    private readonly NotificationStream _stream;
    private readonly ILogger<NotificationEventSubscriber> _logger;

    public NotificationEventSubscriber(IEventBus eventBus, NotificationStream stream, ILogger<NotificationEventSubscriber> logger)
    {
        _eventBus = eventBus;
        _stream = stream;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _eventBus.Subscribe("NotificationRequested", async envelope =>
        {
            try
            {
                var request = System.Text.Json.JsonSerializer.Deserialize<NotificationRequestDTO>(envelope.PayloadJson);
                if (request != null)
                {
                    await _stream.PublishAsync(request, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle notification event.");
            }
        });

        return Task.CompletedTask;
    }
}

