using FluentAssertions;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_WithValidInput_ShouldInitializeDefaults()
    {
        var user = User.Create("demo@tasks.test", "hashedpw");

        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be("demo@tasks.test");
        user.PasswordHash.Should().Be("hashedpw");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldNormalizeEmailToLowercase()
    {
        var user = User.Create("  Demo@Tasks.Test ", "hash");

        user.Email.Should().Be("demo@tasks.test");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@tld")]
    public void Create_WithInvalidEmail_ShouldThrowDomainException(string? email)
    {
        var act = () => User.Create(email!, "hash");

        act.Should().Throw<DomainException>().WithMessage("*email*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingHash_ShouldThrowDomainException(string? hash)
    {
        var act = () => User.Create("demo@tasks.test", hash!);

        act.Should().Throw<DomainException>().WithMessage("*password*");
    }
}
