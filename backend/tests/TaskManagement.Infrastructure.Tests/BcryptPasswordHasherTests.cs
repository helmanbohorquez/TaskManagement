using FluentAssertions;
using TaskManagement.Infrastructure.Security;

namespace TaskManagement.Infrastructure.Tests;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _sut = new();

    [Fact]
    public void Hash_ShouldProduceDifferentOutputEachTime()
    {
        var a = _sut.Hash("Password123!");
        var b = _sut.Hash("Password123!");
        a.Should().NotBe(b);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        var hash = _sut.Hash("Password123!");
        _sut.Verify("Password123!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ShouldReturnFalse()
    {
        var hash = _sut.Hash("Password123!");
        _sut.Verify("Different", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_WithMalformedHash_ShouldReturnFalse()
    {
        _sut.Verify("Password", "not-a-valid-bcrypt-hash").Should().BeFalse();
    }
}
