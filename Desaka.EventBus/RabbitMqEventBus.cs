using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Desaka.Contracts.EventBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Desaka.EventBus;

public sealed class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly EventBusOptions _options;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private readonly IConnection _connection;
    private readonly IModel _publisherChannel;

    public RabbitMqEventBus(IOptions<EventBusOptions> options, ILogger<RabbitMqEventBus> logger)
    {
        _options = options.Value;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.User,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _publisherChannel = _connection.CreateModel();
        _publisherChannel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);
    }

    public Task PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(envelope, EventBusJson.Default.EventEnvelope);
        var props = _publisherChannel.CreateBasicProperties();
        props.DeliveryMode = 2;
        props.ContentType = "application/json";

        _publisherChannel.BasicPublish(_options.Exchange, envelope.EventType, true, props, payload);
        return Task.CompletedTask;
    }

    public IDisposable Subscribe(string routingKey, Func<EventEnvelope, Task> handler)
    {
        var channel = _connection.CreateModel();
        channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);
        var queueName = string.IsNullOrWhiteSpace(_options.QueueName)
            ? $"desaka.{Environment.MachineName.ToLowerInvariant()}"
            : _options.QueueName;

        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queueName, _options.Exchange, routingKey);
        channel.BasicQos(0, _options.PrefetchCount, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, args) =>
        {
            try
            {
                var envelope = JsonSerializer.Deserialize(args.Body.Span, EventBusJson.Default.EventEnvelope);
                if (envelope == null)
                {
                    channel.BasicAck(args.DeliveryTag, false);
                    return;
                }

                await handler(envelope);
                channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Event bus handler failed for {RoutingKey}", routingKey);
                channel.BasicNack(args.DeliveryTag, false, true);
            }
        };

        channel.BasicConsume(queueName, autoAck: false, consumer);
        return new Subscription(channel, _options.Exchange, queueName, routingKey);
    }

    public void Dispose()
    {
        _publisherChannel.Dispose();
        _connection.Dispose();
    }

    private sealed class Subscription : IDisposable
    {
        private readonly IModel _channel;
        private readonly string _exchange;
        private readonly string _queue;
        private readonly string _routingKey;

        public Subscription(IModel channel, string exchange, string queue, string routingKey)
        {
            _channel = channel;
            _exchange = exchange;
            _queue = queue;
            _routingKey = routingKey;
        }

        public void Dispose()
        {
            try
            {
                _channel.QueueUnbind(_queue, _exchange, _routingKey);
            }
            catch
            {
            }

            _channel.Dispose();
        }
    }
}

internal static class EventBusJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    public static readonly EventBusJsonContext Default = new(Options);
}

[JsonSerializable(typeof(EventEnvelope))]
internal sealed partial class EventBusJsonContext : JsonSerializerContext
{
}
