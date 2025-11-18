using FluentValidation;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Validators;

public class CreateCurrencyRequestValidator : AbstractValidator<CreateCurrencyRequest>
{
    public CreateCurrencyRequestValidator()
    {
        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency code must be exactly 3 uppercase letters");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Symbol)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.DecimalPlaces)
            .InclusiveBetween(0, 6);
    }
}

