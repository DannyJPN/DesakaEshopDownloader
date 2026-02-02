using Desaka.DataAccess.Abstractions;
using Desaka.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Desaka.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDesakaDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            options.ConnectionString = configuration.GetConnectionString("Desaka")
                ?? configuration.GetConnectionString("Default");
        }

        services.AddSingleton(options);
        services.AddSingleton<IDbProviderResolver>(new DbProviderResolver(options));

        services.AddDbContext<DesakaDbContext>(builder =>
        {
            var provider = Enum.TryParse<DbProvider>(options.Provider ?? string.Empty, true, out var parsed)
                ? parsed
                : DbProvider.SqlServer;

            var connectionString = options.ConnectionString;
            switch (provider)
            {
                case DbProvider.Sqlite:
                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        var path = string.IsNullOrWhiteSpace(options.SqlitePath)
                            ? "desaka.db"
                            : options.SqlitePath;
                        connectionString = $"Data Source={path}";
                    }
                    builder.UseSqlite(connectionString);
                    break;
                case DbProvider.SqlServer:
                default:
                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        throw new InvalidOperationException("Database connection string is missing.");
                    }
                    builder.UseSqlServer(connectionString);
                    break;
            }
        });

        RegisterRepositories(services);

        services.AddScoped<IUnitOfWork, DesakaUnitOfWork>();
        services.AddScoped<ISecretProtector, AesGcmSecretProtector>();
        services.Configure<SecretProtectorOptions>(configuration.GetSection(SecretProtectorOptions.SectionName));
        services.AddSingleton<IFileService, FileService>();
        services.AddScoped<IRuntimeLockService, RuntimeLockService>();

        return services;
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        var assembly = typeof(DesakaUnitOfWork).Assembly;
        var interfaces = assembly.GetTypes()
            .Where(type => type.IsInterface && type.Name.EndsWith("Repository", StringComparison.Ordinal));

        foreach (var interfaceType in interfaces)
        {
            var implementation = assembly.GetTypes()
                .FirstOrDefault(type => type.IsClass && !type.IsAbstract && interfaceType.IsAssignableFrom(type));

            if (implementation != null)
            {
                services.AddScoped(interfaceType, implementation);
            }
        }
    }
}
