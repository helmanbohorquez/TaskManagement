using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Security;

namespace TaskManagement.Infrastructure.Tests;

public class JwtTokenServiceTests
{
    private readonly JwtOptions _options = new()
    {
        Issuer = "test",
        Audience = "test-audience",
        SigningKey = "this-is-a-test-key-that-is-32-chars!!",
        ExpiryMinutes = 15
    };

    [Fact]
    public void CreateToken_ShouldIncludeUserIdAndEmailClaims()
    {
        var sut = new JwtTokenService(Options.Create(_options));
        var user = User.Create("demo@tasks.test", "hash");

        var result = sut.CreateToken(user);

        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        jwt.Claims.Should().Contain(c => c.Type == "sub" && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == user.Email);
    }

    [Fact]
    public void Constructor_WithShortKey_ShouldThrow()
    {
        var bad = new JwtOptions { SigningKey = "short" };
        var act = () => new JwtTokenService(Options.Create(bad));
        act.Should().Throw<InvalidOperationException>();
    }
}
