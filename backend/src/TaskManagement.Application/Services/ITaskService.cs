using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskResponse>> ListAsync(Guid userId, TaskItemStatus? status, CancellationToken ct = default);
    Task<TaskResponse> GetAsync(Guid userId, Guid taskId, CancellationToken ct = default);
    Task<TaskResponse> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken ct = default);
    Task<TaskResponse> UpdateAsync(Guid userId, Guid taskId, UpdateTaskRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid taskId, CancellationToken ct = default);
}
