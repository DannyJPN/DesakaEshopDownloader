using System.Text;

namespace Desaka.Unifier.Infrastructure;

/// <summary>
/// Generates CSV reports for product changes.
/// </summary>
public interface IPriceChangeReportGenerator
{
    Task GenerateReportsAsync(CombineResult result, string outputDir, CancellationToken ct = default);
}

public sealed class PriceChangeReportGenerator : IPriceChangeReportGenerator
{
    public async Task GenerateReportsAsync(CombineResult result, string outputDir, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outputDir);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        if (result.NewProducts.Count > 0)
            await SaveCsvAsync(Path.Combine(outputDir, $"nove_produkty_{timestamp}.csv"),
                new[] { "kod", "nazev", "url" },
                result.NewProducts.Select(p => new[] { p.Code, p.Name, p.Url }), ct);

        if (result.RemovedProducts.Count > 0)
            await SaveCsvAsync(Path.Combine(outputDir, $"stare_produkty_{timestamp}.csv"),
                new[] { "kod", "nazev", "url" },
                result.RemovedProducts.Select(p => new[] { p.Code, p.Name, p.Url }), ct);

        if (result.PriceIncreases.Count > 0)
            await SaveCsvAsync(Path.Combine(outputDir, $"zdrazeni_{timestamp}.csv"),
                new[] { "nazev", "kod", "stara_cena", "nova_cena", "rozdil", "procenta", "url" },
                result.PriceIncreases.Select(p => new[] {
                    p.Name, p.Code, p.OldPrice.ToString("F2"), p.NewPrice.ToString("F2"),
                    p.Difference.ToString("F2"), p.Percentage.ToString("F1"), p.Url }), ct);

        if (result.PriceDecreases.Count > 0)
            await SaveCsvAsync(Path.Combine(outputDir, $"slevy_{timestamp}.csv"),
                new[] { "nazev", "kod", "stara_cena", "nova_cena", "rozdil", "procenta", "url" },
                result.PriceDecreases.Select(p => new[] {
                    p.Name, p.Code, p.OldPrice.ToString("F2"), p.NewPrice.ToString("F2"),
                    p.Difference.ToString("F2"), p.Percentage.ToString("F1"), p.Url }), ct);

        if (result.CodeChanges.Count > 0)
            await SaveCsvAsync(Path.Combine(outputDir, $"zmeny_kodu_{timestamp}.csv"),
                new[] { "nazev", "stary_kod", "novy_kod", "url" },
                result.CodeChanges.Select(c => new[] { c.Name, c.OldCode, c.NewCode, c.Url }), ct);
    }

    private static async Task SaveCsvAsync(string path, string[] headers, IEnumerable<string[]> rows, CancellationToken ct)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

        foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));

        await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8, ct);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
