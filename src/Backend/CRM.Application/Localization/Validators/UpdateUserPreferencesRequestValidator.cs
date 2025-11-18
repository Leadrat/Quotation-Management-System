using FluentValidation;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Validators;

public class UpdateUserPreferencesRequestValidator : AbstractValidator<UpdateUserPreferencesRequest>
{
    public UpdateUserPreferencesRequestValidator()
    {
        RuleFor(x => x.LanguageCode)
            .MaximumLength(5)
            .When(x => x.LanguageCode != null);

        RuleFor(x => x.CurrencyCode)
            .Length(3)
            .Matches("^[A-Z]{3}$")
            .When(x => !string.IsNullOrEmpty(x.CurrencyCode));

        RuleFor(x => x.DateFormat)
            .MaximumLength(20)
            .When(x => x.DateFormat != null);

        RuleFor(x => x.TimeFormat)
            .Must(f => f == "12h" || f == "24h")
            .When(x => x.TimeFormat != null);

        RuleFor(x => x.NumberFormat)
            .MaximumLength(50)
            .When(x => x.NumberFormat != null);

        RuleFor(x => x.FirstDayOfWeek)
            .InclusiveBetween(0, 6)
            .When(x => x.FirstDayOfWeek.HasValue);
    }
}

