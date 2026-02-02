namespace Desaka.Contracts.Config;

public sealed record PathsConfigDTO(string OutputRoot, string RawRoot, string ImagesRoot, string ReportsRoot, string LogsRoot, string AutopollReportsRoot, string TempRoot);
