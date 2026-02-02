using System.Text.RegularExpressions;
using Desaka.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Desaka.Unifier.Infrastructure;

public sealed class CodeGenerator
{
    private static readonly Regex CodeSuffixRegex = new(@"(\d{4})$", RegexOptions.Compiled);
    private readonly DesakaDbContext _db;
    private readonly MemoryLookupService _memoryLookup;

    public CodeGenerator(DesakaDbContext db, MemoryLookupService memoryLookup)
    {
        _db = db;
        _memoryLookup = memoryLookup;
    }

    public async Task<string> GenerateAsync(string brand, string category, CancellationToken cancellationToken)
    {
        var brandCode = await _memoryLookup.LookupExactAsync("brand_code_list", brand, null, cancellationToken)
            ?? NormalizeSegment(brand, 3);
        var categoryCode = await _memoryLookup.LookupExactAsync("category_code_list", category, null, cancellationToken)
            ?? NormalizeSegment(category, 3);

        var prefix = $"{NormalizeSegment(brandCode, 3)}{NormalizeSegment(categoryCode, 3)}";

        var existing = await _db.ProductsCurrent.AsNoTracking()
            .Where(x => x.Kod.StartsWith(prefix))
            .Select(x => x.Kod)
            .ToListAsync(cancellationToken);

        var max = 0;
        foreach (var code in existing)
        {
            var match = CodeSuffixRegex.Match(code);
            if (!match.Success)
            {
                continue;
            }

            if (int.TryParse(match.Groups[1].Value, out var parsed))
            {
                max = Math.Max(max, parsed);
            }
        }

        var next = max + 1;
        return $"{prefix}{next:0000}";
    }

    private static string NormalizeSegment(string value, int length)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new string('X', length);
        }

        var cleaned = new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (cleaned.Length >= length)
        {
            return cleaned.Substring(0, length);
        }

        return cleaned.PadRight(length, 'X');
    }
}
