using Desaka.DataAccess.Abstractions;

namespace Desaka.DataAccess;

public sealed class DbProviderResolver : IDbProviderResolver
{
    private readonly DatabaseOptions _options;

    public DbProviderResolver(DatabaseOptions options)
    {
        _options = options;
    }

    public DbProvider Current
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_options.Provider))
            {
                return DbProvider.SqlServer;
            }

            return Enum.TryParse<DbProvider>(_options.Provider, true, out var provider)
                ? provider
                : DbProvider.SqlServer;
        }
    }
}
