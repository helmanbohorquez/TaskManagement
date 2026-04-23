using FluentAssertions;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Domain.Tests;

public class TaskItemTests
{
    private static TaskItem NewValidTask() => TaskItem.Create(
        userId: Guid.NewGuid(),
        title: "Buy groceries",
        description: "Milk, eggs, bread",
        dueDate: DateTime.UtcNow.AddDays(1));

    [Fact]
    public void Create_WithValidInput_ShouldInitializeDefaults()
    {
        var userId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(2);

        var task = TaskItem.Create(userId, "Title", "Desc", dueDate);

        task.Id.Should().NotBe(Guid.Empty);
        task.UserId.Should().Be(userId);
        task.Title.Should().Be("Title");
        task.Description.Should().Be("Desc");
        task.Status.Should().Be(TaskItemStatus.Pending);
        task.DueDate.Should().Be(dueDate);
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        task.UpdatedAt.Should().Be(task.CreatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingTitle_ShouldThrowDomainException(string? title)
    {
        var act = () => TaskItem.Create(Guid.NewGuid(), title!, "d", DateTime.UtcNow.AddDays(1));

        act.Should().Throw<DomainException>().WithMessage("*title*");
    }

    [Fact]
    public void Create_WithTooLongTitle_ShouldThrowDomainException()
    {
        var longTitle = new string('a', 201);

        var act = () => TaskItem.Create(Guid.NewGuid(), longTitle, "d", DateTime.UtcNow.AddDays(1));

        act.Should().Throw<DomainException>().WithMessage("*200*");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowDomainException()
    {
        var act = () => TaskItem.Create(Guid.Empty, "t", "d", DateTime.UtcNow.AddDays(1));

        act.Should().Throw<DomainException>().WithMessage("*user*");
    }

    [Fact]
    public void UpdateDetails_ShouldMutateAndTouchUpdatedAt()
    {
        var task = NewValidTask();
        var originalUpdatedAt = task.UpdatedAt;
        Thread.Sleep(5);

        task.UpdateDetails("New title", "New desc", DateTime.UtcNow.AddDays(5));

        task.Title.Should().Be("New title");
        task.Description.Should().Be("New desc");
        task.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatusAndTouchUpdatedAt()
    {
        var task = NewValidTask();
        var originalUpdatedAt = task.UpdatedAt;
        Thread.Sleep(5);

        task.ChangeStatus(TaskItemStatus.InProgress);

        task.Status.Should().Be(TaskItemStatus.InProgress);
        task.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void ChangeStatus_WithUndefinedEnum_ShouldThrowDomainException()
    {
        var task = NewValidTask();

        var act = () => task.ChangeStatus((TaskItemStatus)99);

        act.Should().Throw<DomainException>();
    }
}
