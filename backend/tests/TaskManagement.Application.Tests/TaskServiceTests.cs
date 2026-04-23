using FluentAssertions;
using FluentValidation;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Services;
using TaskManagement.Application.Validation;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Repositories;

namespace TaskManagement.Application.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repo = new();
    private readonly TaskService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskServiceTests()
    {
        _sut = new TaskService(
            _repo.Object,
            new CreateTaskRequestValidator(),
            new UpdateTaskRequestValidator());
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldReturnNewTask()
    {
        var req = new CreateTaskRequest("Buy milk", "2 liters", DateTime.UtcNow.AddDays(1));

        var result = await _sut.CreateAsync(_userId, req);

        result.Title.Should().Be("Buy milk");
        result.Status.Should().Be(TaskItemStatus.Pending);
        _repo.Verify(r => r.AddAsync(It.Is<TaskItem>(t => t.UserId == _userId && t.Title == "Buy milk"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyTitle_ShouldThrowValidationException()
    {
        var req = new CreateTaskRequest("", null, DateTime.UtcNow.AddDays(1));

        var act = () => _sut.CreateAsync(_userId, req);

        await act.Should().ThrowAsync<ValidationException>();
        _repo.Verify(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_WhenTaskMissing_ShouldThrowNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync((TaskItem?)null);

        var act = () => _sut.GetAsync(_userId, Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAsync_WhenTaskExists_ShouldReturnResponse()
    {
        var task = TaskItem.Create(_userId, "t", "d", DateTime.UtcNow.AddDays(1));
        _repo.Setup(r => r.GetByIdAsync(task.Id, _userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(task);

        var result = await _sut.GetAsync(_userId, task.Id);

        result.Id.Should().Be(task.Id);
        result.Title.Should().Be("t");
    }

    [Fact]
    public async Task UpdateAsync_ShouldApplyFieldsAndPersist()
    {
        var task = TaskItem.Create(_userId, "old", "old-desc", DateTime.UtcNow.AddDays(1));
        _repo.Setup(r => r.GetByIdAsync(task.Id, _userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(task);

        var newDue = DateTime.UtcNow.AddDays(3);
        var req = new UpdateTaskRequest("new", "new-desc", newDue, TaskItemStatus.InProgress);

        var result = await _sut.UpdateAsync(_userId, task.Id, req);

        result.Title.Should().Be("new");
        result.Status.Should().Be(TaskItemStatus.InProgress);
        _repo.Verify(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskMissing_ShouldThrowNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync((TaskItem?)null);
        var req = new UpdateTaskRequest("t", "d", DateTime.UtcNow.AddDays(1), TaskItemStatus.Pending);

        var act = () => _sut.UpdateAsync(_userId, Guid.NewGuid(), req);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenRepoReturnsFalse_ShouldThrowNotFound()
    {
        _repo.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);

        var act = () => _sut.DeleteAsync(_userId, Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenRepoReturnsTrue_ShouldSucceed()
    {
        _repo.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        await _sut.DeleteAsync(_userId, Guid.NewGuid());
    }

    [Fact]
    public async Task ListAsync_ShouldPassStatusFilterToRepo()
    {
        _repo.Setup(r => r.ListByUserAsync(_userId, TaskItemStatus.Done, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<TaskItem>());

        await _sut.ListAsync(_userId, TaskItemStatus.Done);

        _repo.Verify(r => r.ListByUserAsync(_userId, TaskItemStatus.Done, It.IsAny<CancellationToken>()), Times.Once);
    }
}
