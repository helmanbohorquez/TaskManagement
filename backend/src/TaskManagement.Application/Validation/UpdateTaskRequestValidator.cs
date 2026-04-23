using FluentValidation;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Validation;

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(TaskItem.MaxTitleLength);

        RuleFor(x => x.Description)
            .MaximumLength(TaskItem.MaxDescriptionLength);

        RuleFor(x => x.DueDate)
            .NotEqual(default(DateTime)).WithMessage("Due date is required.")
            .Must(d => d.Date >= DateTime.UtcNow.Date)
                .WithMessage("Due date cannot be in the past.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status.");
    }
}
