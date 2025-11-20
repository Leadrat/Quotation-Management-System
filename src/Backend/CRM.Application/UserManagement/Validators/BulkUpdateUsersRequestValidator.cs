using FluentValidation;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Validators;

public class BulkUpdateUsersRequestValidator : AbstractValidator<BulkUpdateUsersRequest>
{
    public BulkUpdateUsersRequestValidator()
    {
        RuleFor(x => x.UserIds)
            .NotEmpty().WithMessage("At least one user ID must be provided")
            .Must(ids => ids.Count <= 100).WithMessage("Cannot update more than 100 users at once");
    }
}

