using FluentValidation;
using CRM.Application.UserManagement.Requests;

namespace CRM.Application.UserManagement.Validators;

public class SetOutOfOfficeRequestValidator : AbstractValidator<SetOutOfOfficeRequest>
{
    public SetOutOfOfficeRequestValidator()
    {
        RuleFor(x => x.Message)
            .MaximumLength(1000).WithMessage("Out-of-office message must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Message));
    }
}

