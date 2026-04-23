using FluentAssertions;
using Microsoft.Data.Sqlite;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Security;

namespace TaskManagement.Infrastructure.Tests;

public class DbInitializerTests
{
    [Fact]
    public async Task Initialize_ShouldCreateTables()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"taskmgmt-init-{Guid.NewGuid()}.db");
        try
        {
            var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
            var sut = new DbInitializer(factory, new BcryptPasswordHasher());

            await sut.InitializeAsync();

            await using var conn = factory.Create();
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name IN ('Users','Tasks');";
            var tables = new List<string>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) tables.Add(reader.GetString(0));
            tables.Should().BeEquivalentTo(new[] { "Users", "Tasks" });
        }
        finally
        {
            try { File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task SeedDemoData_ShouldBeIdempotent()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"taskmgmt-seed-{Guid.NewGuid()}.db");
        try
        {
            var factory = new SqliteConnectionFactory($"Data Source={dbPath}");
            var sut = new DbInitializer(factory, new BcryptPasswordHasher());
            await sut.InitializeAsync();

            await sut.SeedDemoDataAsync();
            await sut.SeedDemoDataAsync();

            await using var conn = factory.Create();
            await conn.OpenAsync();
            var countUsers = conn.CreateCommand();
            countUsers.CommandText = "SELECT COUNT(*) FROM Users;";
            var u = (long)(await countUsers.ExecuteScalarAsync())!;
            var countTasks = conn.CreateCommand();
            countTasks.CommandText = "SELECT COUNT(*) FROM Tasks;";
            var t = (long)(await countTasks.ExecuteScalarAsync())!;

            u.Should().Be(1);
            t.Should().Be(4);
        }
        finally
        {
            try { File.Delete(dbPath); } catch { }
        }
    }
}
