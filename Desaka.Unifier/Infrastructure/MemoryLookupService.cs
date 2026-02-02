using Desaka.DataAccess;
using Desaka.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desaka.Unifier.Infrastructure;

public sealed class MemoryLookupService
{
    private static readonly IReadOnlyDictionary<string, Type> MemoryTypeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
    {
        { "brand_code_list", typeof(MemoryBrandCodeList) },
        { "category_code_list", typeof(MemoryCategoryCodeList) },
        { "category_sub_code_list", typeof(MemoryCategorySubCodeList) },
        { "category_id_list", typeof(MemoryCategoryIdList) },
        { "category_list", typeof(MemoryCategoryList) },
        { "item_filter", typeof(MemoryItemFilter) },
        { "wrongs", typeof(MemoryWrongs) },
        { "category_mapping_glami", typeof(MemoryCategoryMappingGlami) },
        { "category_mapping_google", typeof(MemoryCategoryMappingGoogle) },
        { "category_mapping_heureka", typeof(MemoryCategoryMappingHeureka) },
        { "category_mapping_zbozi", typeof(MemoryCategoryMappingZbozi) },
        { "category_memory", typeof(MemoryCategoryMemory) },
        { "category_name_memory", typeof(MemoryCategoryNameMemory) },
        { "desc_memory", typeof(MemoryDescMemory) },
        { "keywords_google", typeof(MemoryKeywordsGoogle) },
        { "keywords_zbozi", typeof(MemoryKeywordsZbozi) },
        { "name_memory", typeof(MemoryNameMemory) },
        { "product_brand_memory", typeof(MemoryProductBrandMemory) },
        { "product_model_memory", typeof(MemoryProductModelMemory) },
        { "product_type_memory", typeof(MemoryProductTypeMemory) },
        { "short_desc_memory", typeof(MemoryShortDescMemory) },
        { "variant_name_memory", typeof(MemoryVariantNameMemory) },
        { "variant_value_memory", typeof(MemoryVariantValueMemory) },
        { "stock_status_memory", typeof(MemoryStockStatusMemory) }
    };

    private readonly DesakaDbContext _db;

    public MemoryLookupService(DesakaDbContext db)
    {
        _db = db;
    }

    public async Task<string?> LookupExactAsync(string memoryType, string key, string? languageCode, CancellationToken cancellationToken)
    {
        var type = ResolveType(memoryType);
        var query = _db.Set(type).AsQueryable();
        if (!string.IsNullOrWhiteSpace(languageCode) && type.GetProperty("LanguageCode") != null)
        {
            query = query.Where(entity => EF.Property<string>(entity, "LanguageCode") == languageCode);
        }

        query = query.Where(entity => EF.Property<string>(entity, "Key") == key);
        var entity = await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        return entity == null ? null : (string?)type.GetProperty("Value")?.GetValue(entity);
    }

    public async Task<string?> LookupContainsAsync(string memoryType, string key, string? languageCode, CancellationToken cancellationToken)
    {
        var type = ResolveType(memoryType);
        var query = _db.Set(type).AsQueryable();
        if (!string.IsNullOrWhiteSpace(languageCode) && type.GetProperty("LanguageCode") != null)
        {
            query = query.Where(entity => EF.Property<string>(entity, "LanguageCode") == languageCode);
        }

        query = query.Where(entity => EF.Property<string>(entity, "Key").Contains(key));
        var entity = await query.AsNoTracking().OrderBy(entity => EF.Property<int>(entity, "Id")).FirstOrDefaultAsync(cancellationToken);
        return entity == null ? null : (string?)type.GetProperty("Value")?.GetValue(entity);
    }

    public async Task AddOrUpdateAsync(string memoryType, string key, string value, string? languageCode, string source, CancellationToken cancellationToken)
    {
        var type = ResolveType(memoryType);
        var set = _db.Set(type);
        var query = set.AsQueryable().Where(entity => EF.Property<string>(entity, "Key") == key);
        if (!string.IsNullOrWhiteSpace(languageCode) && type.GetProperty("LanguageCode") != null)
        {
            query = query.Where(entity => EF.Property<string>(entity, "LanguageCode") == languageCode);
        }

        var entity = await query.FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
        {
            entity = Activator.CreateInstance(type) ?? throw new InvalidOperationException("Cannot create memory entity.");
            type.GetProperty("Key")?.SetValue(entity, key);
            if (type.GetProperty("LanguageCode") != null)
            {
                type.GetProperty("LanguageCode")?.SetValue(entity, languageCode ?? string.Empty);
            }

            type.GetProperty("CreatedAt")?.SetValue(entity, DateTime.UtcNow);
            set.Add(entity);
        }

        type.GetProperty("Value")?.SetValue(entity, value);
        type.GetProperty("Source")?.SetValue(entity, source);
        type.GetProperty("UpdatedAt")?.SetValue(entity, DateTime.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static Type ResolveType(string memoryType)
    {
        if (MemoryTypeMap.TryGetValue(memoryType, out var type))
        {
            return type;
        }

        throw new InvalidOperationException($"Unknown memory type '{memoryType}'.");
    }
}
