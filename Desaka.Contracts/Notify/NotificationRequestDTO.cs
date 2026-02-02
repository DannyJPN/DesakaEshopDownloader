using Desaka.Contracts.Common;

namespace Desaka.Contracts.Notify;

public sealed record NotificationRequestDTO(string EventType, string Title, string Message, NotificationSeverity Severity, string? DataJson = null, string? CorrelationId = null);
