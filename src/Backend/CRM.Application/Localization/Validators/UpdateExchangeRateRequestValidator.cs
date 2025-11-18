using FluentValidation;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Validators;

public class UpdateExchangeRateRequestValidator : AbstractValidator<UpdateExchangeRateRequest>
{
    public UpdateExchangeRateRequestValidator()
    {
        RuleFor(x => x.FromCurrencyCode)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Z]{3}$");

        RuleFor(x => x.ToCurrencyCode)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Z]{3}$")
            .NotEqual(x => x.FromCurrencyCode)
            .WithMessage("From and To currency codes must be different");

        RuleFor(x => x.Rate)
            .GreaterThan(0);

        RuleFor(x => x.EffectiveDate)
            .NotEmpty();

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.EffectiveDate)
            .When(x => x.ExpiryDate.HasValue);
    }
}

