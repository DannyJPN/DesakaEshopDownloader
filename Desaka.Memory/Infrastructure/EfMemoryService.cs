using System.Reflection;
using Desaka.Contracts.Memory;
using Desaka.DataAccess;
using Desaka.DataAccess.Entities;
using Desaka.Memory.Application;
using Microsoft.EntityFrameworkCore;

namespace Desaka.Memory.Infrastructure;

public sealed class EfMemoryService : IMemoryService
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
        { "stock_status_memory", typeof(MemoryStockStatusMemory) },

        { "memory_brand_code_list", typeof(MemoryBrandCodeList) },
        { "memory_category_code_list", typeof(MemoryCategoryCodeList) },
        { "memory_category_sub_code_list", typeof(MemoryCategorySubCodeList) },
        { "memory_category_id_list", typeof(MemoryCategoryIdList) },
        { "memory_category_list", typeof(MemoryCategoryList) },
        { "memory_item_filter", typeof(MemoryItemFilter) },
        { "memory_wrongs", typeof(MemoryWrongs) },
        { "memory_category_mapping_glami", typeof(MemoryCategoryMappingGlami) },
        { "memory_category_mapping_google", typeof(MemoryCategoryMappingGoogle) },
        { "memory_category_mapping_heureka", typeof(MemoryCategoryMappingHeureka) },
        { "memory_category_mapping_zbozi", typeof(MemoryCategoryMappingZbozi) },
        { "memory_category_memory", typeof(MemoryCategoryMemory) },
        { "memory_category_name_memory", typeof(MemoryCategoryNameMemory) },
        { "memory_desc_memory", typeof(MemoryDescMemory) },
        { "memory_keywords_google", typeof(MemoryKeywordsGoogle) },
        { "memory_keywords_zbozi", typeof(MemoryKeywordsZbozi) },
        { "memory_name_memory", typeof(MemoryNameMemory) },
        { "memory_product_brand_memory", typeof(MemoryProductBrandMemory) },
        { "memory_product_model_memory", typeof(MemoryProductModelMemory) },
        { "memory_product_type_memory", typeof(MemoryProductTypeMemory) },
        { "memory_short_desc_memory", typeof(MemoryShortDescMemory) },
        { "memory_variant_name_memory", typeof(MemoryVariantNameMemory) },
        { "memory_variant_value_memory", typeof(MemoryVariantValueMemory) },
        { "memory_stock_status_memory", typeof(MemoryStockStatusMemory) }
    };

    private readonly DesakaDbContext _db;

    public EfMemoryService(DesakaDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<MemoryEntryDTO>> SearchAsync(MemorySearchRequestDTO request, CancellationToken cancellationToken = default)
    {
        var type = ResolveType(request.MemoryType);
        var set = QuerySet(type);

        var query = request.Query ?? string.Empty;
        var keyProp = type.GetProperty("Key");
        var valueProp = type.GetProperty("Value");
        var languageProp = type.GetProperty("LanguageCode");

        if (keyProp == null || valueProp == null)
        {
            return Array.Empty<MemoryEntryDTO>();
        }

        if (!string.IsNullOrWhiteSpace(request.LanguageCode) && languageProp != null)
        {
            set = set.Where(entity => EF.Property<string>(entity, "LanguageCode") == request.LanguageCode);
        }

        if (request.Mode.Equals("exact", StringComparison.OrdinalIgnoreCase))
        {
            set = set.Where(entity => EF.Property<string>(entity, "Key") == query);
        }
        else
        {
            set = set.Where(entity => EF.Property<string>(entity, "Key").Contains(query));
        }

        var list = await set.AsNoTracking().ToListAsync(cancellationToken);
        return list.Select(ToDto).ToList();
    }

    public async Task<MemoryEntryDTO> CreateAsync(string memoryType, MemoryEntryDTO entry, CancellationToken cancellationToken = default)
    {
        var type = ResolveType(memoryType);
        var entity = Activator.CreateInstance(type) ?? throw new InvalidOperationException("Cannot create entity.");
        ApplyDto(entity, entry);

        _db.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<MemoryEntryDTO?> UpdateAsync(string memoryType, MemoryEntryDTO entry, CancellationToken cancellationToken = default)
    {
        var type = ResolveType(memoryType);
        var entity = await _db.FindAsync(type, new object[] { entry.Id }, cancellationToken);
        if (entity == null)
        {
            return null;
        }

        ApplyDto(entity, entry);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(string memoryType, int id, CancellationToken cancellationToken = default)
    {
        var type = ResolveType(memoryType);
        var entity = await _db.FindAsync(type, new object[] { id }, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _db.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<MemoryEntryDTO>> ExportAsync(string memoryType, string? languageCode, CancellationToken cancellationToken = default)
    {
        var type = ResolveType(memoryType);
        var set = QuerySet(type);

        if (!string.IsNullOrWhiteSpace(languageCode) && type.GetProperty("LanguageCode") != null)
        {
            set = set.Where(entity => EF.Property<string>(entity, "LanguageCode") == languageCode);
        }

        var list = await set.AsNoTracking().ToListAsync(cancellationToken);
        return list.Select(ToDto).ToList();
    }

    public async Task<MemoryImportResponseDTO> ImportAsync(string memoryType, Stream fileStream, string? languageCode, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(fileStream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var rows = new List<string[]>();
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            rows.Add(ParseCsvLine(line));
        }

        if (rows.Count == 0)
        {
            return new MemoryImportResponseDTO(0, Array.Empty<string>());
        }

        var header = rows[0];
        var type = ResolveType(memoryType);
        var imported = 0;
        for (var i = 1; i < rows.Count; i++)
        {
            var entity = Activator.CreateInstance(type) ?? throw new InvalidOperationException("Cannot create entity.");
            MapRowToEntity(header, rows[i], entity, languageCode);
            _db.Add(entity);
            imported++;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new MemoryImportResponseDTO(imported, Array.Empty<string>());
    }

    private static Type ResolveType(string memoryType)
    {
        if (MemoryTypeMap.TryGetValue(memoryType, out var type))
        {
            return type;
        }

        throw new InvalidOperationException($"Unknown memory type '{memoryType}'.");
    }

    private IQueryable<object> QuerySet(Type type)
    {
        var method = typeof(DbContext).GetMethods()
            .First(m => m.Name == nameof(DbContext.Set)
                && m.IsGenericMethodDefinition
                && m.GetParameters().Length == 0);
        var set = method.MakeGenericMethod(type).Invoke(_db, null);
        return ((IQueryable)set!).Cast<object>();
    }

    private static MemoryEntryDTO ToDto(object entity)
    {
        var type = entity.GetType();
        var id = (int)(type.GetProperty("Id")?.GetValue(entity) ?? 0);
        var key = (string?)(type.GetProperty("Key")?.GetValue(entity)) ?? string.Empty;
        var value = (string?)(type.GetProperty("Value")?.GetValue(entity)) ?? string.Empty;
        var source = (string?)(type.GetProperty("Source")?.GetValue(entity)) ?? string.Empty;
        var createdAt = (DateTime)(type.GetProperty("CreatedAt")?.GetValue(entity) ?? DateTime.UtcNow);
        var updatedAt = (DateTime)(type.GetProperty("UpdatedAt")?.GetValue(entity) ?? DateTime.UtcNow);
        var language = (string?)type.GetProperty("LanguageCode")?.GetValue(entity);

        return new MemoryEntryDTO(id, key, value, source, createdAt, updatedAt, language);
    }

    private static void ApplyDto(object entity, MemoryEntryDTO entry)
    {
        var type = entity.GetType();
        type.GetProperty("Key")?.SetValue(entity, entry.Key);
        type.GetProperty("Value")?.SetValue(entity, entry.Value);
        type.GetProperty("Source")?.SetValue(entity, entry.Source);
        type.GetProperty("CreatedAt")?.SetValue(entity, entry.CreatedAt == default ? DateTime.UtcNow : entry.CreatedAt);
        type.GetProperty("UpdatedAt")?.SetValue(entity, DateTime.UtcNow);
        if (type.GetProperty("LanguageCode") != null)
        {
            type.GetProperty("LanguageCode")?.SetValue(entity, entry.LanguageCode ?? string.Empty);
        }
    }

    private static void MapRowToEntity(string[] header, string[] row, object entity, string? languageCode)
    {
        var type = entity.GetType();
        for (var i = 0; i < header.Length && i < row.Length; i++)
        {
            var column = header[i].Trim();
            if (string.IsNullOrWhiteSpace(column))
            {
                continue;
            }

            var propName = ColumnToProperty(column);
            var prop = type.GetProperty(propName);
            if (prop == null || !prop.CanWrite)
            {
                continue;
            }

            var value = row[i];
            prop.SetValue(entity, ConvertValue(value, prop.PropertyType));
        }

        if (!string.IsNullOrWhiteSpace(languageCode) && type.GetProperty("LanguageCode") != null)
        {
            type.GetProperty("LanguageCode")?.SetValue(entity, languageCode);
        }

        type.GetProperty("UpdatedAt")?.SetValue(entity, DateTime.UtcNow);
    }

    private static string ColumnToProperty(string column)
    {
        return column switch
        {
            "id" => "Id",
            "key" => "Key",
            "value" => "Value",
            "source" => "Source",
            "created_at" => "CreatedAt",
            "updated_at" => "UpdatedAt",
            "language_code" => "LanguageCode",
            "url_domain" => "UrlDomain",
            _ => column
        };
    }

    private static object? ConvertValue(string value, Type type)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var targetType = Nullable.GetUnderlyingType(type) ?? type;
        if (targetType == typeof(string))
        {
            return value;
        }

        if (targetType == typeof(int))
        {
            return int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(DateTime))
        {
            return DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal);
        }

        return Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var builder = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    builder.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (ch == ';' && !inQuotes)
            {
                values.Add(builder.ToString());
                builder.Clear();
            }
            else
            {
                builder.Append(ch);
            }
        }

        values.Add(builder.ToString());
        return values.ToArray();
    }
}

