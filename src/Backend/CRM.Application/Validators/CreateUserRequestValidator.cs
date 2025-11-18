using CRM.Shared.DTOs;
using FluentValidation;

namespace CRM.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().MaximumLength(255).EmailAddress();

        RuleFor(r => r.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[!@#$%^&*(),.?\\\"{}|<>]");

        RuleFor(r => r.FirstName)
            .NotEmpty().MinimumLength(2).MaximumLength(100)
            .Matches("^[a-zA-Z\\s\\-']+$");

        RuleFor(r => r.LastName)
            .NotEmpty().MinimumLength(2).MaximumLength(100)
            .Matches("^[a-zA-Z\\s\\-']+$");

        RuleFor(r => r.Mobile)
            .Matches("^\\+[1-9]\\d{1,14}$").When(r => !string.IsNullOrEmpty(r.Mobile));

        RuleFor(r => r.PhoneCode)
            .Matches("^\\+\\d{1,3}$").When(r => !string.IsNullOrEmpty(r.PhoneCode));

        RuleFor(r => r)
            .Must(PhoneCodeMatchesMobile).WithMessage("PhoneCode must match Mobile country code")
            .When(r => !string.IsNullOrEmpty(r.Mobile) && !string.IsNullOrEmpty(r.PhoneCode));

        RuleFor(r => r.RoleId).NotEmpty();
    }

    private bool PhoneCodeMatchesMobile(CreateUserRequest r)
    {
        var mobile = r.Mobile!;
        int i = 1;
        while (i < mobile.Length && i <= 4 && char.IsDigit(mobile[i])) i++;
        var derived = mobile.Substring(0, i);
        return string.Equals(derived, r.PhoneCode, System.StringComparison.Ordinal);
    }
}
