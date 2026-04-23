using FluentAssertions;
using FluentValidation;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Security;
using TaskManagement.Application.Services;
using TaskManagement.Application.Validation;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Repositories;

namespace TaskManagement.Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _users.Object,
            _hasher.Object,
            _tokens.Object,
            new RegisterRequestValidator(),
            new LoginRequestValidator());
    }

    [Fact]
    public async Task RegisterAsync_WithValidInput_ShouldCreateUserAndReturnToken()
    {
        _users.Setup(u => u.GetByEmailAsync("demo@tasks.test", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Hash("Demo1234")).Returns("HASH");
        _tokens.Setup(t => t.CreateToken(It.IsAny<User>()))
               .Returns(new TokenResult("jwt", DateTime.UtcNow.AddHours(1)));

        var result = await _sut.RegisterAsync(new RegisterRequest("Demo@tasks.test", "Demo1234"));

        result.Token.Should().Be("jwt");
        result.Email.Should().Be("demo@tasks.test");
        _users.Verify(u => u.AddAsync(It.Is<User>(x => x.Email == "demo@tasks.test" && x.PasswordHash == "HASH"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowConflict()
    {
        var existing = User.Create("demo@tasks.test", "hash");
        _users.Setup(u => u.GetByEmailAsync("demo@tasks.test", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var act = () => _sut.RegisterAsync(new RegisterRequest("demo@tasks.test", "Demo1234"));

        await act.Should().ThrowAsync<ConflictException>();
        _users.Verify(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("not-an-email", "Demo1234")]
    [InlineData("demo@tasks.test", "short")]
    [InlineData("demo@tasks.test", "alllower1")]
    [InlineData("demo@tasks.test", "ALLUPPER1")]
    [InlineData("demo@tasks.test", "NoDigitsHere")]
    public async Task RegisterAsync_WithInvalidInput_ShouldThrowValidation(string email, string password)
    {
        var act = () => _sut.RegisterAsync(new RegisterRequest(email, password));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        var user = User.Create("demo@tasks.test", "HASH");
        _users.Setup(u => u.GetByEmailAsync("demo@tasks.test", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Demo1234", "HASH")).Returns(true);
        _tokens.Setup(t => t.CreateToken(user)).Returns(new TokenResult("jwt", DateTime.UtcNow.AddHours(1)));

        var result = await _sut.LoginAsync(new LoginRequest("demo@tasks.test", "Demo1234"));

        result.Token.Should().Be("jwt");
    }

    [Fact]
    public async Task LoginAsync_WhenUserMissing_ShouldThrowUnauthorized()
    {
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var act = () => _sut.LoginAsync(new LoginRequest("demo@tasks.test", "Demo1234"));

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordWrong_ShouldThrowUnauthorized()
    {
        var user = User.Create("demo@tasks.test", "HASH");
        _users.Setup(u => u.GetByEmailAsync("demo@tasks.test", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Demo1234", "HASH")).Returns(false);

        var act = () => _sut.LoginAsync(new LoginRequest("demo@tasks.test", "Demo1234"));

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
