using FluentValidation;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Validators;

public class UpdateTaskStatusRequestValidator : AbstractValidator<UpdateTaskStatusRequest>
{
    public UpdateTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(s => s == "Pending" || s == "InProgress" || s == "Completed" || s == "Cancelled")
            .WithMessage("Status must be one of: Pending, InProgress, Completed, Cancelled");
    }
}

