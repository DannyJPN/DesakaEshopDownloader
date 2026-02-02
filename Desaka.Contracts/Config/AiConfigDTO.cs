namespace Desaka.Contracts.Config;

public sealed record AiConfigDTO(string TaskName, int ProviderId, string ModelName, string ApiKeyPlaintext, bool IsEnabled);
