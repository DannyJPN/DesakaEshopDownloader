namespace Desaka.Contracts.EventBus;

public sealed record EventEnvelope(string EventType, string SourceService, DateTime CreatedAtUtc, string? CorrelationId, string PayloadJson);
