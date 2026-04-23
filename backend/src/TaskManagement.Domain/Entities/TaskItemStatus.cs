namespace TaskManagement.Domain.Entities;

public enum TaskItemStatus
{
    Pending = 0,
    InProgress = 1,
    Done = 2,
    // Expired is never persisted; it is a computed, presentation-only status
    // derived from DueDate + stored Status. Writes with Status = Expired are
    // rejected by the update validator and by TaskItem.ChangeStatus.
    Expired = 3
}
