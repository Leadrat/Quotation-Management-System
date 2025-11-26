using FluentValidation;
using CRM.Application.TaxManagement.Requests;
using System.Text.RegularExpressions;

namespace CRM.Application.TaxManagement.Validators
{
    public class UpdateCountryRequestValidator : AbstractValidator<UpdateCountryRequest>
    {
        public UpdateCountryRequestValidator()
        {
            RuleFor(x => x.CountryName)
                .MinimumLength(2)
                .When(x => !string.IsNullOrWhiteSpace(x.CountryName))
                .WithMessage("Country name must be at least 2 characters")
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.CountryName))
                .WithMessage("Country name cannot exceed 100 characters");

            RuleFor(x => x.CountryCode)
                .Length(2)
                .When(x => !string.IsNullOrWhiteSpace(x.CountryCode))
                .WithMessage("Country code must be exactly 2 characters")
                .Matches(new Regex("^[A-Z]{2}$"))
                .When(x => !string.IsNullOrWhiteSpace(x.CountryCode))
                .WithMessage("Country code must be 2 uppercase letters (ISO 3166-1 alpha-2)");

            RuleFor(x => x.DefaultCurrency)
                .Length(3)
                .When(x => !string.IsNullOrWhiteSpace(x.DefaultCurrency))
                .WithMessage("Default currency must be exactly 3 characters")
                .Matches(new Regex("^[A-Z]{3}$"))
                .When(x => !string.IsNullOrWhiteSpace(x.DefaultCurrency))
                .WithMessage("Default currency must be 3 uppercase letters (ISO 4217)");
        }
    }
}

