using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Security;

public interface ITokenService
{
    TokenResult CreateToken(User user);
}

public record TokenResult(string Token, DateTime ExpiresAt);
