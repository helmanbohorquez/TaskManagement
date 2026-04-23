using Microsoft.Data.Sqlite;
using TaskManagement.Application.Security;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence;

public class DbInitializer
{
    private readonly ISqliteConnectionFactory _factory;
    private readonly IPasswordHasher _hasher;

    public DbInitializer(ISqliteConnectionFactory factory, IPasswordHasher hasher)
    {
        _factory = factory;
        _hasher = hasher;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await ExecuteAsync(conn, @"
            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY NOT NULL,
                Email TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );", ct);

        await ExecuteAsync(conn, @"
            CREATE TABLE IF NOT EXISTS Tasks (
                Id TEXT PRIMARY KEY NOT NULL,
                UserId TEXT NOT NULL,
                Title TEXT NOT NULL,
                Description TEXT NOT NULL,
                Status INTEGER NOT NULL,
                DueDate TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
            );", ct);

        await ExecuteAsync(conn, @"
            CREATE INDEX IF NOT EXISTS IX_Tasks_UserId ON Tasks(UserId);", ct);
    }

    public async Task SeedDemoDataAsync(CancellationToken ct = default)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        const string demoEmail = "demo@tasks.test";
        const string demoPassword = "Demo123!";

        var existsCmd = conn.CreateCommand();
        existsCmd.CommandText = "SELECT Id FROM Users WHERE Email = $email LIMIT 1;";
        existsCmd.Parameters.AddWithValue("$email", demoEmail);
        var existingId = await existsCmd.ExecuteScalarAsync(ct);
        if (existingId is not null)
            return;

        var user = User.Create(demoEmail, _hasher.Hash(demoPassword));

        using var tx = conn.BeginTransaction();
        try
        {
            var insertUser = conn.CreateCommand();
            insertUser.Transaction = tx;
            insertUser.CommandText = @"
                INSERT INTO Users (Id, Email, PasswordHash, CreatedAt)
                VALUES ($id, $email, $hash, $createdAt);";
            insertUser.Parameters.AddWithValue("$id", user.Id.ToString());
            insertUser.Parameters.AddWithValue("$email", user.Email);
            insertUser.Parameters.AddWithValue("$hash", user.PasswordHash);
            insertUser.Parameters.AddWithValue("$createdAt", user.CreatedAt.ToString("O"));
            await insertUser.ExecuteNonQueryAsync(ct);

            var seedTasks = new[]
            {
                TaskItem.Create(user.Id, "Welcome to Task Manager", "Explore the app and create your first task.", DateTime.UtcNow.AddDays(1)),
                TaskItem.Create(user.Id, "Write weekly report", "Summarize progress and blockers.", DateTime.UtcNow.AddDays(3)),
                TaskItem.Create(user.Id, "Plan sprint demo", "Prepare slides and recording.", DateTime.UtcNow.AddDays(5))
            };
            seedTasks[1].ChangeStatus(TaskItemStatus.InProgress);
            seedTasks[2].ChangeStatus(TaskItemStatus.Done);

            foreach (var t in seedTasks)
            {
                var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO Tasks (Id, UserId, Title, Description, Status, DueDate, CreatedAt, UpdatedAt)
                    VALUES ($id, $userId, $title, $desc, $status, $due, $created, $updated);";
                cmd.Parameters.AddWithValue("$id", t.Id.ToString());
                cmd.Parameters.AddWithValue("$userId", t.UserId.ToString());
                cmd.Parameters.AddWithValue("$title", t.Title);
                cmd.Parameters.AddWithValue("$desc", t.Description);
                cmd.Parameters.AddWithValue("$status", (int)t.Status);
                cmd.Parameters.AddWithValue("$due", t.DueDate.ToString("O"));
                cmd.Parameters.AddWithValue("$created", t.CreatedAt.ToString("O"));
                cmd.Parameters.AddWithValue("$updated", t.UpdatedAt.ToString("O"));
                await cmd.ExecuteNonQueryAsync(ct);
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private static async Task ExecuteAsync(SqliteConnection conn, string sql, CancellationToken ct)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
