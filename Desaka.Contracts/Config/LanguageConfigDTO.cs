namespace Desaka.Contracts.Config;

public sealed record LanguageConfigDTO(string Code, string Name, bool IsEnabled, bool IsDefault);
