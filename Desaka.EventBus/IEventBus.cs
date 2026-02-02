using Desaka.Contracts.EventBus;

namespace Desaka.EventBus;

public interface IEventBus
{
    Task PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken = default);
    IDisposable Subscribe(string routingKey, Func<EventEnvelope, Task> handler);
}
