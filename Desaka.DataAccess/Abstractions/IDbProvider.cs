namespace Desaka.DataAccess.Abstractions;

public enum DbProvider
{
    SqlServer = 0,
    Sqlite = 1,
    Postgres = 2
}

public interface IDbProviderResolver
{
    DbProvider Current { get; }
}
