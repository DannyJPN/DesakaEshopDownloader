namespace Desaka.Unifying.Models;

public sealed class Variant
{
    public Dictionary<string, string> KeyValuePairs { get; } = new(StringComparer.OrdinalIgnoreCase);
    public decimal? CurrentPrice { get; set; }
    public decimal? BasicPrice { get; set; }
    public string? StockStatus { get; set; }
    public string VariantCode { get; set; } = "";
}
