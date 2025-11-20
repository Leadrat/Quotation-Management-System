using FluentValidation;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Validators;

public class CreateTeamRequestValidator : AbstractValidator<CreateTeamRequest>
{
    public CreateTeamRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required")
            .MaximumLength(255).WithMessage("Team name must not exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.TeamLeadUserId)
            .NotEmpty().WithMessage("Team lead is required");

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Company ID is required");
    }
}

