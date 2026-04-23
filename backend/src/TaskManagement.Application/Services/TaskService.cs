using FluentValidation;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Repositories;

namespace TaskManagement.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly IValidator<CreateTaskRequest> _createValidator;
    private readonly IValidator<UpdateTaskRequest> _updateValidator;

    public TaskService(
        ITaskRepository repository,
        IValidator<CreateTaskRequest> createValidator,
        IValidator<UpdateTaskRequest> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<TaskResponse>> ListAsync(Guid userId, TaskItemStatus? status, CancellationToken ct = default)
    {
        // Expired is not a stored value, so a DB filter by status=Expired would
        // return nothing. Fetch all tasks and filter in memory by the same rule
        // TaskResponse.FromEntity uses (not Done and due date in the past).
        if (status == TaskItemStatus.Expired)
        {
            var all = await _repository.ListByUserAsync(userId, null, ct);
            return all
                .Select(TaskResponse.FromEntity)
                .Where(t => t.IsExpired)
                .ToList();
        }

        var tasks = await _repository.ListByUserAsync(userId, status, ct);
        return tasks.Select(TaskResponse.FromEntity).ToList();
    }

    public async Task<TaskResponse> GetAsync(Guid userId, Guid taskId, CancellationToken ct = default)
    {
        var task = await _repository.GetByIdAsync(taskId, userId, ct)
            ?? throw new NotFoundException($"Task '{taskId}' was not found.");
        return TaskResponse.FromEntity(task);
    }

    public async Task<TaskResponse> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken ct = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, ct);

        var task = TaskItem.Create(userId, request.Title, request.Description, request.DueDate);
        await _repository.AddAsync(task, ct);
        return TaskResponse.FromEntity(task);
    }

    public async Task<TaskResponse> UpdateAsync(Guid userId, Guid taskId, UpdateTaskRequest request, CancellationToken ct = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, ct);

        var task = await _repository.GetByIdAsync(taskId, userId, ct)
            ?? throw new NotFoundException($"Task '{taskId}' was not found.");

        task.UpdateDetails(request.Title, request.Description, request.DueDate);
        task.ChangeStatus(request.Status);

        await _repository.UpdateAsync(task, ct);
        return TaskResponse.FromEntity(task);
    }

    public async Task DeleteAsync(Guid userId, Guid taskId, CancellationToken ct = default)
    {
        var deleted = await _repository.DeleteAsync(taskId, userId, ct);
        if (!deleted)
            throw new NotFoundException($"Task '{taskId}' was not found.");
    }
}
