using FluentAssertions;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Tests;

public class UserRepositoryTests : IClassFixture<SqliteFixture>
{
    private readonly UserRepository _repo;

    public UserRepositoryTests(SqliteFixture fx)
    {
        _repo = new UserRepository(fx.Factory);
    }

    [Fact]
    public async Task Add_then_GetByEmail_ShouldReturnUser()
    {
        var email = $"u-{Guid.NewGuid()}@tasks.test";
        var u = User.Create(email, "hash");
        await _repo.AddAsync(u);

        var fetched = await _repo.GetByEmailAsync(email);

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(u.Id);
        fetched.PasswordHash.Should().Be("hash");
    }

    [Fact]
    public async Task GetByEmail_ShouldBeCaseInsensitive()
    {
        var email = $"u-{Guid.NewGuid()}@tasks.test";
        var u = User.Create(email, "hash");
        await _repo.AddAsync(u);

        var fetched = await _repo.GetByEmailAsync(email.ToUpperInvariant());

        fetched.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_ShouldReturnUser()
    {
        var u = User.Create($"u-{Guid.NewGuid()}@tasks.test", "hash");
        await _repo.AddAsync(u);

        var fetched = await _repo.GetByIdAsync(u.Id);

        fetched.Should().NotBeNull();
        fetched!.Email.Should().Be(u.Email);
    }
}
