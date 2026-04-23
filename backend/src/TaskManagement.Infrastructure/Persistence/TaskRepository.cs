using System.Globalization;
using Microsoft.Data.Sqlite;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Repositories;

namespace TaskManagement.Infrastructure.Persistence;

public class TaskRepository : ITaskRepository
{
    private readonly ISqliteConnectionFactory _factory;

    public TaskRepository(ISqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, UserId, Title, Description, Status, DueDate, CreatedAt, UpdatedAt
            FROM Tasks
            WHERE Id = $id AND UserId = $userId
            LIMIT 1;";
        cmd.Parameters.AddWithValue("$id", id.ToString());
        cmd.Parameters.AddWithValue("$userId", userId.ToString());
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<TaskItem>> ListByUserAsync(Guid userId, TaskItemStatus? status, CancellationToken ct = default)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = status.HasValue
            ? @"SELECT Id, UserId, Title, Description, Status, DueDate, CreatedAt, UpdatedAt
                FROM Tasks WHERE UserId = $userId AND Status = $status
                ORDER BY DueDate ASC;"
            : @"SELECT Id, UserId, Title, Description, Status, DueDate, CreatedAt, UpdatedAt
                FROM Tasks WHERE UserId = $userId
                ORDER BY DueDate ASC;";
        cmd.Parameters.AddWithValue("$userId", userId.ToString());
        if (status.HasValue)
            cmd.Parameters.AddWithValue("$status", (int)status.Value);

        var result = new List<TaskItem>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            result.Add(Map(reader));
        return result;
    }

    public async Task AddAsync(TaskItem task, CancellationToken ct = default)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Tasks (Id, UserId, Title, Description, Status, DueDate, CreatedAt, UpdatedAt)
            VALUES ($id, $userId, $title, $desc, $status, $due, $created, $updated);";
        Bind(cmd, task);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Tasks
            SET Title = $title, Description = $desc, Status = $status, DueDate = $due, UpdatedAt = $updated
            WHERE Id = $id AND UserId = $userId;";
        Bind(cmd, task);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Tasks WHERE Id = $id AND UserId = $userId;";
        cmd.Parameters.AddWithValue("$id", id.ToString());
        cmd.Parameters.AddWithValue("$userId", userId.ToString());
        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    private static void Bind(SqliteCommand cmd, TaskItem t)
    {
        cmd.Parameters.AddWithValue("$id", t.Id.ToString());
        cmd.Parameters.AddWithValue("$userId", t.UserId.ToString());
        cmd.Parameters.AddWithValue("$title", t.Title);
        cmd.Parameters.AddWithValue("$desc", t.Description);
        cmd.Parameters.AddWithValue("$status", (int)t.Status);
        cmd.Parameters.AddWithValue("$due", t.DueDate.ToString("O"));
        cmd.Parameters.AddWithValue("$created", t.CreatedAt.ToString("O"));
        cmd.Parameters.AddWithValue("$updated", t.UpdatedAt.ToString("O"));
    }

    private static TaskItem Map(SqliteDataReader r) =>
        TaskItem.Rehydrate(
            id: Guid.Parse(r.GetString(0)),
            userId: Guid.Parse(r.GetString(1)),
            title: r.GetString(2),
            description: r.GetString(3),
            status: (TaskItemStatus)r.GetInt32(4),
            dueDate: DateTime.Parse(r.GetString(5), null, DateTimeStyles.RoundtripKind),
            createdAt: DateTime.Parse(r.GetString(6), null, DateTimeStyles.RoundtripKind),
            updatedAt: DateTime.Parse(r.GetString(7), null, DateTimeStyles.RoundtripKind));
}
