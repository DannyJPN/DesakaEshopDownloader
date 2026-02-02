using Desaka.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Desaka.Validation.Infrastructure;

public sealed class MemoryValidationIssue
{
    public string Rule { get; set; } = "";
    public string Message { get; set; } = "";
    public string? Key { get; set; }
}

public sealed class MemoryValidationService
{
    private readonly DesakaDbContext _db;

    public MemoryValidationService(DesakaDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<MemoryValidationIssue>> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var issues = new List<MemoryValidationIssue>();

        var brandCodes = await _db.MemoryBrandCodeList.AsNoTracking().Select(x => x.Value).ToListAsync(cancellationToken);
        var brandMemory = await _db.MemoryProductBrandMemory.AsNoTracking().Select(x => x.Value).ToListAsync(cancellationToken);

        foreach (var brand in brandMemory)
        {
            if (!brandCodes.Contains(brand, StringComparer.OrdinalIgnoreCase))
            {
                issues.Add(new MemoryValidationIssue
                {
                    Rule = "ProductBrandMemory vs BrandCodeList",
                    Message = "Brand missing in BrandCodeList",
                    Key = brand
                });
            }
        }

        var typeMemory = await _db.MemoryProductTypeMemory.AsNoTracking().Select(x => x.Value).ToListAsync(cancellationToken);
        var modelMemory = await _db.MemoryProductModelMemory.AsNoTracking().Select(x => x.Value).ToListAsync(cancellationToken);

        foreach (var type in typeMemory)
        {
            if (brandCodes.Any(b => type.Contains(b, StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(new MemoryValidationIssue { Rule = "ProductType contains Brand", Message = "ProductType contains Brand", Key = type });
            }
        }

        foreach (var model in modelMemory)
        {
            if (brandCodes.Any(b => model.Contains(b, StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(new MemoryValidationIssue { Rule = "ProductModel contains Brand", Message = "ProductModel contains Brand", Key = model });
            }
        }

        var overlap = typeMemory.Intersect(modelMemory, StringComparer.OrdinalIgnoreCase).ToList();
        foreach (var value in overlap)
        {
            issues.Add(new MemoryValidationIssue { Rule = "ProductType vs ProductModel overlap", Message = "Overlap value", Key = value });
        }

        var variantNames = await _db.MemoryVariantNameMemory.AsNoTracking().Select(x => x.Value).ToListAsync(cancellationToken);
        var variantValues = await _db.MemoryVariantValueMemory.AsNoTracking().Select(x => x.Value).ToListAsync(cancellationToken);

        foreach (var type in typeMemory)
        {
            if (variantNames.Any(v => ContainsWord(type, v)) || variantValues.Any(v => ContainsWord(type, v)))
            {
                issues.Add(new MemoryValidationIssue { Rule = "ProductType contains Variant", Message = "Type contains variant term", Key = type });
            }
        }

        foreach (var model in modelMemory)
        {
            if (variantNames.Any(v => ContainsWord(model, v)) || variantValues.Any(v => ContainsWord(model, v)))
            {
                issues.Add(new MemoryValidationIssue { Rule = "ProductModel contains Variant", Message = "Model contains variant term", Key = model });
            }
        }

        var googleKeywords = await _db.MemoryKeywordsGoogle.AsNoTracking().Select(x => x.Value).ToListAsync(cancellationToken);
        var zboziKeywords = await _db.MemoryKeywordsZbozi.AsNoTracking().Select(x => x.Value).ToListAsync(cancellationToken);

        foreach (var kw in googleKeywords.Concat(zboziKeywords))
        {
            if (kw.Contains("|", StringComparison.Ordinal))
            {
                issues.Add(new MemoryValidationIssue { Rule = "Keywords format", Message = "Invalid separator", Key = kw });
            }
        }

        return issues;
    }

    private static bool ContainsWord(string text, string word)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(word))
        {
            return false;
        }

        return System.Text.RegularExpressions.Regex.IsMatch(text, $"\\b{System.Text.RegularExpressions.Regex.Escape(word)}\\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
