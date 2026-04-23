using Microsoft.Data.Sqlite;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Repositories;

namespace TaskManagement.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly ISqliteConnectionFactory _factory;

    public UserRepository(ISqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Email, PasswordHash, CreatedAt FROM Users WHERE Email = $email LIMIT 1;";
        cmd.Parameters.AddWithValue("$email", email.Trim().ToLowerInvariant());
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Email, PasswordHash, CreatedAt FROM Users WHERE Id = $id LIMIT 1;";
        cmd.Parameters.AddWithValue("$id", id.ToString());
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Users (Id, Email, PasswordHash, CreatedAt)
            VALUES ($id, $email, $hash, $createdAt);";
        cmd.Parameters.AddWithValue("$id", user.Id.ToString());
        cmd.Parameters.AddWithValue("$email", user.Email);
        cmd.Parameters.AddWithValue("$hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("$createdAt", user.CreatedAt.ToString("O"));
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static User Map(SqliteDataReader r) =>
        User.Rehydrate(
            id: Guid.Parse(r.GetString(0)),
            email: r.GetString(1),
            passwordHash: r.GetString(2),
            createdAt: DateTime.Parse(r.GetString(3), null, System.Globalization.DateTimeStyles.RoundtripKind));
}
