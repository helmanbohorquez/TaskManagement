using TaskManagement.Application.Security;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Security;

namespace TaskManagement.Infrastructure.Tests;

public sealed class SqliteFixture : IDisposable
{
    public string DbPath { get; }
    public SqliteConnectionFactory Factory { get; }
    public IPasswordHasher Hasher { get; } = new BcryptPasswordHasher();

    public SqliteFixture()
    {
        DbPath = Path.Combine(Path.GetTempPath(), $"taskmgmt-{Guid.NewGuid()}.db");
        Factory = new SqliteConnectionFactory($"Data Source={DbPath}");
        var init = new DbInitializer(Factory, Hasher);
        init.InitializeAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        try { File.Delete(DbPath); } catch { /* best-effort */ }
    }
}
