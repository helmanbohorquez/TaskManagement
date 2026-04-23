using Microsoft.Data.Sqlite;

namespace TaskManagement.Infrastructure.Persistence;

public interface ISqliteConnectionFactory
{
    SqliteConnection Create();
}

public class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        _connectionString = connectionString;
    }

    public SqliteConnection Create() => new(_connectionString);
}
