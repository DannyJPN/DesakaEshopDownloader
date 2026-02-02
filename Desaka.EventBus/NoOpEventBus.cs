using Desaka.Contracts.EventBus;

namespace Desaka.EventBus;

public sealed class NoOpEventBus : IEventBus
{
    public Task PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public IDisposable Subscribe(string routingKey, Func<EventEnvelope, Task> handler)
        => new NoOpSubscription();

    private sealed class NoOpSubscription : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
