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
    DateTime UpdatedAt)
{
    public static TaskResponse FromEntity(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.DueDate,
        task.CreatedAt,
        task.UpdatedAt);
}
