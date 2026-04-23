using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Domain.Entities;

public class TaskItem
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TaskItemStatus Status { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TaskItem() { }

    public static TaskItem Create(Guid userId, string title, string? description, DateTime dueDate)
    {
        if (userId == Guid.Empty)
            throw new DomainException("A task must belong to a user.");

        ValidateTitle(title);
        ValidateDescription(description);

        var now = DateTime.UtcNow;
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title.Trim(),
            Description = (description ?? string.Empty).Trim(),
            Status = TaskItemStatus.Pending,
            DueDate = dueDate,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public static TaskItem Rehydrate(
        Guid id,
        Guid userId,
        string title,
        string description,
        TaskItemStatus status,
        DateTime dueDate,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new TaskItem
        {
            Id = id,
            UserId = userId,
            Title = title,
            Description = description,
            Status = status,
            DueDate = dueDate,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public void UpdateDetails(string title, string? description, DateTime dueDate)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        Title = title.Trim();
        Description = (description ?? string.Empty).Trim();
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(TaskItemStatus status)
    {
        if (!Enum.IsDefined(status))
            throw new DomainException($"Unknown status '{status}'.");
        if (status == TaskItemStatus.Expired)
            throw new DomainException("Expired is a computed status and cannot be set manually.");

        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Task title is required.");
        if (title.Length > MaxTitleLength)
            throw new DomainException($"Task title cannot exceed {MaxTitleLength} characters.");
    }

    private static void ValidateDescription(string? description)
    {
        if (description is { Length: > MaxDescriptionLength })
            throw new DomainException($"Task description cannot exceed {MaxDescriptionLength} characters.");
    }
}
