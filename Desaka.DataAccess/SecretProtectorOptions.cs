namespace Desaka.DataAccess;

public sealed class SecretProtectorOptions
{
    public const string SectionName = "Secrets";
    public string? Base64Key { get; set; }
}
