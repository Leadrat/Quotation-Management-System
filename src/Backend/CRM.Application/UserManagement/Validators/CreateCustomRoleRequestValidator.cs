using FluentValidation;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Validators;

public class CreateCustomRoleRequestValidator : AbstractValidator<CreateCustomRoleRequest>
{
    public CreateCustomRoleRequestValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(255).WithMessage("Role name must not exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("Permissions list cannot be null");
    }
}

