using Desaka.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Desaka.DataAccess;

public sealed class DesakaDbContext : DbContext
{
    public DesakaDbContext(DbContextOptions<DesakaDbContext> options) : base(options)
    {
    }

    public DbSet<ProductsCurrent> ProductsCurrent => Set<ProductsCurrent>();
    public DbSet<ProductsHistory> ProductsHistory => Set<ProductsHistory>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<VariantOption> VariantOptions => Set<VariantOption>();

    public DbSet<ScrapeRun> ScrapeRuns => Set<ScrapeRun>();
    public DbSet<ScrapeRawMetadata> ScrapeRawMetadata => Set<ScrapeRawMetadata>();
    public DbSet<DownloadedProduct> DownloadedProducts => Set<DownloadedProduct>();
    public DbSet<DownloadedGallery> DownloadedGalleries => Set<DownloadedGallery>();
    public DbSet<DownloadedVariant> DownloadedVariants => Set<DownloadedVariant>();
    public DbSet<DownloadedVariantOption> DownloadedVariantOptions => Set<DownloadedVariantOption>();

    public DbSet<AutopollRun> AutopollRuns => Set<AutopollRun>();
    public DbSet<AutopollSnapshot> AutopollSnapshots => Set<AutopollSnapshot>();
    public DbSet<AutopollChangeLog> AutopollChangeLogs => Set<AutopollChangeLog>();
    public DbSet<AutopollBatchCommit> AutopollBatchCommits => Set<AutopollBatchCommit>();
    public DbSet<UnifierRun> UnifierRuns => Set<UnifierRun>();
    public DbSet<UnifierApproval> UnifierApprovals => Set<UnifierApproval>();

    public DbSet<ConfigEshop> ConfigEshops => Set<ConfigEshop>();
    public DbSet<ConfigAutopollRule> ConfigAutopollRules => Set<ConfigAutopollRule>();
    public DbSet<ConfigSetting> ConfigSettings => Set<ConfigSetting>();
    public DbSet<ConfigLanguage> ConfigLanguages => Set<ConfigLanguage>();
    public DbSet<ConfigAiProviderCatalog> ConfigAiProviderCatalogs => Set<ConfigAiProviderCatalog>();
    public DbSet<ConfigAiProviderAssignment> ConfigAiProviderAssignments => Set<ConfigAiProviderAssignment>();
    public DbSet<ConfigAiPricing> ConfigAiPricings => Set<ConfigAiPricing>();
    public DbSet<ConfigPincesobchodApiKey> ConfigPincesobchodApiKeys => Set<ConfigPincesobchodApiKey>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AiUsageLog> AiUsageLogs => Set<AiUsageLog>();

    public DbSet<RuntimeLock> RuntimeLocks => Set<RuntimeLock>();
    public DbSet<RuntimeState> RuntimeStates => Set<RuntimeState>();

    public DbSet<NotifyDeviceToken> NotifyDeviceTokens => Set<NotifyDeviceToken>();

    public DbSet<MemoryBrandCodeList> MemoryBrandCodeList => Set<MemoryBrandCodeList>();
    public DbSet<MemoryCategoryCodeList> MemoryCategoryCodeList => Set<MemoryCategoryCodeList>();
    public DbSet<MemoryCategorySubCodeList> MemoryCategorySubCodeList => Set<MemoryCategorySubCodeList>();
    public DbSet<MemoryCategoryIdList> MemoryCategoryIdList => Set<MemoryCategoryIdList>();
    public DbSet<MemoryCategoryList> MemoryCategoryList => Set<MemoryCategoryList>();
    public DbSet<MemoryItemFilter> MemoryItemFilter => Set<MemoryItemFilter>();
    public DbSet<MemoryWrongs> MemoryWrongs => Set<MemoryWrongs>();
    public DbSet<MemoryCategoryMappingGlami> MemoryCategoryMappingGlami => Set<MemoryCategoryMappingGlami>();
    public DbSet<MemoryCategoryMappingGoogle> MemoryCategoryMappingGoogle => Set<MemoryCategoryMappingGoogle>();
    public DbSet<MemoryCategoryMappingHeureka> MemoryCategoryMappingHeureka => Set<MemoryCategoryMappingHeureka>();
    public DbSet<MemoryCategoryMappingZbozi> MemoryCategoryMappingZbozi => Set<MemoryCategoryMappingZbozi>();
    public DbSet<MemoryCategoryMemory> MemoryCategoryMemory => Set<MemoryCategoryMemory>();
    public DbSet<MemoryCategoryNameMemory> MemoryCategoryNameMemory => Set<MemoryCategoryNameMemory>();
    public DbSet<MemoryDescMemory> MemoryDescMemory => Set<MemoryDescMemory>();
    public DbSet<MemoryKeywordsGoogle> MemoryKeywordsGoogle => Set<MemoryKeywordsGoogle>();
    public DbSet<MemoryKeywordsZbozi> MemoryKeywordsZbozi => Set<MemoryKeywordsZbozi>();
    public DbSet<MemoryNameMemory> MemoryNameMemory => Set<MemoryNameMemory>();
    public DbSet<MemoryProductBrandMemory> MemoryProductBrandMemory => Set<MemoryProductBrandMemory>();
    public DbSet<MemoryProductModelMemory> MemoryProductModelMemory => Set<MemoryProductModelMemory>();
    public DbSet<MemoryProductTypeMemory> MemoryProductTypeMemory => Set<MemoryProductTypeMemory>();
    public DbSet<MemoryShortDescMemory> MemoryShortDescMemory => Set<MemoryShortDescMemory>();
    public DbSet<MemoryVariantNameMemory> MemoryVariantNameMemory => Set<MemoryVariantNameMemory>();
    public DbSet<MemoryVariantValueMemory> MemoryVariantValueMemory => Set<MemoryVariantValueMemory>();
    public DbSet<MemoryStockStatusMemory> MemoryStockStatusMemory => Set<MemoryStockStatusMemory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ApplyTableMappings(modelBuilder);
        ApplyColumnConventions(modelBuilder);
        ApplyIndexes(modelBuilder);
    }

    private static void ApplyTableMappings(ModelBuilder modelBuilder)
    {
        var map = new Dictionary<Type, (string Schema, string Table)>
        {
            { typeof(ProductsCurrent), ("data", "products_current") },
            { typeof(ProductsHistory), ("data", "products_history") },
            { typeof(ProductVariant), ("data", "product_variant") },
            { typeof(VariantOption), ("data", "variant_option") },

            { typeof(ScrapeRun), ("scrape", "scrape_run") },
            { typeof(ScrapeRawMetadata), ("scrape", "scrape_raw_metadata") },
            { typeof(DownloadedProduct), ("scrape", "downloaded_product") },
            { typeof(DownloadedGallery), ("scrape", "downloaded_gallery") },
            { typeof(DownloadedVariant), ("scrape", "downloaded_variant") },
            { typeof(DownloadedVariantOption), ("scrape", "downloaded_variant_option") },

            { typeof(AutopollRun), ("autopoll", "autopoll_run") },
            { typeof(AutopollSnapshot), ("autopoll", "autopoll_snapshot") },
            { typeof(AutopollChangeLog), ("autopoll", "autopoll_change_log") },
            { typeof(AutopollBatchCommit), ("autopoll", "autopoll_batch_commit") },
            { typeof(UnifierRun), ("data", "unifier_run") },
            { typeof(UnifierApproval), ("data", "unifier_approval") },

            { typeof(ConfigEshop), ("config", "config_eshop") },
            { typeof(ConfigAutopollRule), ("config", "config_autopoll_rule") },
            { typeof(ConfigSetting), ("config", "config_settings") },
            { typeof(ConfigLanguage), ("config", "config_languages") },
            { typeof(ConfigAiProviderCatalog), ("config", "config_ai_provider_catalog") },
            { typeof(ConfigAiProviderAssignment), ("config", "config_ai_provider_assignment") },
            { typeof(ConfigAiPricing), ("config", "config_ai_pricing") },
            { typeof(ConfigPincesobchodApiKey), ("config", "config_pincesobchod_api_keys") },

            { typeof(AuditLog), ("audit", "audit_log") },
            { typeof(AiUsageLog), ("audit", "ai_usage_log") },

            { typeof(RuntimeLock), ("runtime", "runtime_lock") },
            { typeof(RuntimeState), ("runtime", "runtime_state") },

            { typeof(NotifyDeviceToken), ("notify", "notify_device_token") },

            { typeof(MemoryBrandCodeList), ("memory", "memory_brand_code_list") },
            { typeof(MemoryCategoryCodeList), ("memory", "memory_category_code_list") },
            { typeof(MemoryCategorySubCodeList), ("memory", "memory_category_sub_code_list") },
            { typeof(MemoryCategoryIdList), ("memory", "memory_category_id_list") },
            { typeof(MemoryCategoryList), ("memory", "memory_category_list") },
            { typeof(MemoryItemFilter), ("memory", "memory_item_filter") },
            { typeof(MemoryWrongs), ("memory", "memory_wrongs") },
            { typeof(MemoryCategoryMappingGlami), ("memory", "memory_category_mapping_glami") },
            { typeof(MemoryCategoryMappingGoogle), ("memory", "memory_category_mapping_google") },
            { typeof(MemoryCategoryMappingHeureka), ("memory", "memory_category_mapping_heureka") },
            { typeof(MemoryCategoryMappingZbozi), ("memory", "memory_category_mapping_zbozi") },
            { typeof(MemoryCategoryMemory), ("memory", "memory_category_memory") },
            { typeof(MemoryCategoryNameMemory), ("memory", "memory_category_name_memory") },
            { typeof(MemoryDescMemory), ("memory", "memory_desc_memory") },
            { typeof(MemoryKeywordsGoogle), ("memory", "memory_keywords_google") },
            { typeof(MemoryKeywordsZbozi), ("memory", "memory_keywords_zbozi") },
            { typeof(MemoryNameMemory), ("memory", "memory_name_memory") },
            { typeof(MemoryProductBrandMemory), ("memory", "memory_product_brand_memory") },
            { typeof(MemoryProductModelMemory), ("memory", "memory_product_model_memory") },
            { typeof(MemoryProductTypeMemory), ("memory", "memory_product_type_memory") },
            { typeof(MemoryShortDescMemory), ("memory", "memory_short_desc_memory") },
            { typeof(MemoryVariantNameMemory), ("memory", "memory_variant_name_memory") },
            { typeof(MemoryVariantValueMemory), ("memory", "memory_variant_value_memory") },
            { typeof(MemoryStockStatusMemory), ("memory", "memory_stock_status_memory") }
        };

        foreach (var entry in map)
        {
            modelBuilder.Entity(entry.Key).ToTable(entry.Value.Table, entry.Value.Schema);
        }
    }

    private static void ApplyColumnConventions(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                var name = ToColumnName(property.Name);
                property.SetColumnName(name);
            }
        }
    }

    private static void ApplyIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductsCurrent>()
            .HasIndex(x => x.Kod)
            .IsUnique();

        modelBuilder.Entity<ProductVariant>()
            .HasIndex(x => x.Variantcode)
            .IsUnique();

        modelBuilder.Entity<VariantOption>()
            .HasIndex(x => new { x.VariantId, x.OptionName, x.OptionValue })
            .IsUnique();

        modelBuilder.Entity<DownloadedVariantOption>()
            .HasIndex(x => new { x.VariantId, x.OptionName, x.OptionValue })
            .IsUnique();
    }

    private static string ToColumnName(string propertyName)
    {
        if (propertyName.Equals("PriceInputPer1K", StringComparison.Ordinal))
        {
            return "price_input_per_1k";
        }

        if (propertyName.Equals("PriceOutputPer1K", StringComparison.Ordinal))
        {
            return "price_output_per_1k";
        }

        if (propertyName.StartsWith("ZboziCz", StringComparison.Ordinal))
        {
            return "zbozicz_" + ToSnakeCase(propertyName.Substring("ZboziCz".Length));
        }

        if (propertyName.StartsWith("HeurekaCz", StringComparison.Ordinal))
        {
            return "heurekacz_" + ToSnakeCase(propertyName.Substring("HeurekaCz".Length));
        }

        if (propertyName.StartsWith("GlamiSk", StringComparison.Ordinal))
        {
            return "glamisk_" + ToSnakeCase(propertyName.Substring("GlamiSk".Length));
        }

        return ToSnakeCase(propertyName);
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var builder = new System.Text.StringBuilder(value.Length + 8);
        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];
            if (char.IsUpper(ch))
            {
                if (i > 0)
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(ch));
            }
            else if (char.IsDigit(ch))
            {
                if (i > 0 && value[i - 1] != '_' && !char.IsDigit(value[i - 1]))
                {
                    builder.Append('_');
                }

                builder.Append(ch);
            }
            else
            {
                builder.Append(ch);
            }
        }

        return builder.ToString();
    }
}
