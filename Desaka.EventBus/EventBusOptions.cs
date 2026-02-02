namespace Desaka.EventBus;

public sealed class EventBusOptions
{
    public const string SectionName = "EventBus";
    public bool Enabled { get; set; } = true;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string User { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = "desaka.events";
    public string? QueueName { get; set; }
    public ushort PrefetchCount { get; set; } = 50;
}
