using FluentValidation;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Validators;

public class AssignTaskRequestValidator : AbstractValidator<AssignTaskRequest>
{
    public AssignTaskRequestValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required")
            .Must(et => et == "Quotation" || et == "Approval" || et == "Client")
            .WithMessage("Entity type must be one of: Quotation, Approval, Client");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required");

        RuleFor(x => x.AssignedToUserId)
            .NotEmpty().WithMessage("Assigned to user ID is required");
    }
}

