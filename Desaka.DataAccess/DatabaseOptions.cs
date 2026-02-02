namespace Desaka.DataAccess;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    public string? Provider { get; set; }
    public string? ConnectionString { get; set; }
    public string? SqlitePath { get; set; }
}
