using CRM.Shared.DTOs;
using FluentValidation;

namespace CRM.Application.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(r => r.FirstName)
            .MinimumLength(2).MaximumLength(100)
            .Matches("^[a-zA-Z\\s\\-']+$")
            .When(r => !string.IsNullOrEmpty(r.FirstName));

        RuleFor(r => r.LastName)
            .MinimumLength(2).MaximumLength(100)
            .Matches("^[a-zA-Z\\s\\-']+$")
            .When(r => !string.IsNullOrEmpty(r.LastName));

        RuleFor(r => r.Mobile)
            .Matches("^\\+[1-9]\\d{1,14}$")
            .When(r => !string.IsNullOrEmpty(r.Mobile));

        RuleFor(r => r.PhoneCode)
            .Matches("^\\+\\d{1,3}$")
            .When(r => !string.IsNullOrEmpty(r.PhoneCode));

        RuleFor(r => r)
            .Must(PhoneCodeMatchesMobile)
            .WithMessage("PhoneCode must match Mobile country code")
            .When(r => !string.IsNullOrEmpty(r.Mobile) && !string.IsNullOrEmpty(r.PhoneCode));
    }

    private bool PhoneCodeMatchesMobile(UpdateUserRequest r)
    {
        var mobile = r.Mobile!;
        int i = 1;
        while (i < mobile.Length && i <= 4 && char.IsDigit(mobile[i])) i++;
        var derived = mobile.Substring(0, i);
        return string.Equals(derived, r.PhoneCode, System.StringComparison.Ordinal);
    }
}
