using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.DTOs;

public record CreateTaskRequest(string Title, string? Description, DateTime DueDate);

public record UpdateTaskRequest(string Title, string? Description, DateTime DueDate, TaskItemStatus Status);

public record TaskResponse(
    Guid Id,
    string Title,
    string Description,
    TaskItemStatus Status,
    DateTime DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsExpired)
{
    public static TaskResponse FromEntity(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.DueDate,
        task.CreatedAt,
        task.UpdatedAt,
        IsExpiredOn(task, DateTime.UtcNow));

    private static bool IsExpiredOn(TaskItem task, DateTime nowUtc) =>
        task.Status != TaskItemStatus.Done && task.DueDate.Date < nowUtc.Date;
}
