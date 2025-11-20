using FluentValidation;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Validators;

public class AddTeamMemberRequestValidator : AbstractValidator<AddTeamMemberRequest>
{
    public AddTeamMemberRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(role => role == "Member" || role == "Lead" || role == "Admin")
            .WithMessage("Role must be one of: Member, Lead, Admin");
    }
}

