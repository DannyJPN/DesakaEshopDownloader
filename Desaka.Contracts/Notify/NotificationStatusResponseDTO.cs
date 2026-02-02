namespace Desaka.Contracts.Notify;

public sealed record NotificationStatusResponseDTO(bool Enabled, IReadOnlyList<string> Channels);
