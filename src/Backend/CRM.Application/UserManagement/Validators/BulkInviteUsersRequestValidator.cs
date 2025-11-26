using FluentValidation;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Validators;

public class BulkInviteUsersRequestValidator : AbstractValidator<BulkInviteUsersRequest>
{
    public BulkInviteUsersRequestValidator()
    {
        RuleFor(x => x.Users)
            .NotEmpty().WithMessage("At least one user must be provided")
            .Must(users => users.Count <= 100).WithMessage("Cannot invite more than 100 users at once");

        RuleForEach(x => x.Users)
            .SetValidator(new BulkInviteUserItemValidator());
    }
}

public class BulkInviteUserItemValidator : AbstractValidator<BulkInviteUserItem>
{
    public BulkInviteUserItemValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");
    }
}

