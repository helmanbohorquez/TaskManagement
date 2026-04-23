using System.Text.RegularExpressions;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Domain.Entities;

public partial class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash)
    {
        var normalized = NormalizeEmail(email);
        if (!IsValidEmail(normalized))
            throw new DomainException("A valid email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("A password hash is required.");

        return new User
        {
            Id = Guid.NewGuid(),
            Email = normalized,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User Rehydrate(Guid id, string email, string passwordHash, DateTime createdAt)
    {
        return new User
        {
            Id = id,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = createdAt
        };
    }

    private static string NormalizeEmail(string? email) =>
        (email ?? string.Empty).Trim().ToLowerInvariant();

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return EmailRegex().IsMatch(email);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
