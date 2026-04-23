using FluentAssertions;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Tests;

public class TaskRepositoryTests : IClassFixture<SqliteFixture>
{
    private readonly SqliteFixture _fx;
    private readonly TaskRepository _repo;
    private readonly UserRepository _userRepo;

    public TaskRepositoryTests(SqliteFixture fx)
    {
        _fx = fx;
        _repo = new TaskRepository(_fx.Factory);
        _userRepo = new UserRepository(_fx.Factory);
    }

    private async Task<User> SeedUserAsync()
    {
        var u = User.Create($"user-{Guid.NewGuid()}@tasks.test", "hash");
        await _userRepo.AddAsync(u);
        return u;
    }

    [Fact]
    public async Task Add_then_GetById_ShouldReturnEntity()
    {
        var u = await SeedUserAsync();
        var t = TaskItem.Create(u.Id, "t", "d", DateTime.UtcNow.AddDays(1));

        await _repo.AddAsync(t);
        var fetched = await _repo.GetByIdAsync(t.Id, u.Id);

        fetched.Should().NotBeNull();
        fetched!.Title.Should().Be("t");
        fetched.UserId.Should().Be(u.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenAccessedByDifferentUser()
    {
        var u1 = await SeedUserAsync();
        var u2 = await SeedUserAsync();
        var t = TaskItem.Create(u1.Id, "t", "d", DateTime.UtcNow.AddDays(1));
        await _repo.AddAsync(t);

        var fetched = await _repo.GetByIdAsync(t.Id, u2.Id);

        fetched.Should().BeNull();
    }

    [Fact]
    public async Task ListByUser_ShouldFilterByUserAndStatus()
    {
        var u = await SeedUserAsync();
        var pending = TaskItem.Create(u.Id, "p", "d", DateTime.UtcNow.AddDays(1));
        var done = TaskItem.Create(u.Id, "d", "d", DateTime.UtcNow.AddDays(2));
        done.ChangeStatus(TaskItemStatus.Done);
        await _repo.AddAsync(pending);
        await _repo.AddAsync(done);

        var all = await _repo.ListByUserAsync(u.Id, null);
        var doneOnly = await _repo.ListByUserAsync(u.Id, TaskItemStatus.Done);

        all.Should().HaveCount(2);
        doneOnly.Should().ContainSingle(t => t.Id == done.Id);
    }

    [Fact]
    public async Task Update_ShouldPersistChanges()
    {
        var u = await SeedUserAsync();
        var t = TaskItem.Create(u.Id, "t", "d", DateTime.UtcNow.AddDays(1));
        await _repo.AddAsync(t);

        t.UpdateDetails("new-title", "new-desc", DateTime.UtcNow.AddDays(5));
        t.ChangeStatus(TaskItemStatus.InProgress);
        await _repo.UpdateAsync(t);

        var fetched = await _repo.GetByIdAsync(t.Id, u.Id);
        fetched!.Title.Should().Be("new-title");
        fetched.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public async Task Delete_ShouldRemoveAndReturnTrue()
    {
        var u = await SeedUserAsync();
        var t = TaskItem.Create(u.Id, "t", "d", DateTime.UtcNow.AddDays(1));
        await _repo.AddAsync(t);

        var deleted = await _repo.DeleteAsync(t.Id, u.Id);

        deleted.Should().BeTrue();
        (await _repo.GetByIdAsync(t.Id, u.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_ShouldReturnFalse_WhenNotOwner()
    {
        var u1 = await SeedUserAsync();
        var u2 = await SeedUserAsync();
        var t = TaskItem.Create(u1.Id, "t", "d", DateTime.UtcNow.AddDays(1));
        await _repo.AddAsync(t);

        var deleted = await _repo.DeleteAsync(t.Id, u2.Id);

        deleted.Should().BeFalse();
        (await _repo.GetByIdAsync(t.Id, u1.Id)).Should().NotBeNull();
    }
}
