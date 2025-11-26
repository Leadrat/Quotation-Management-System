using FluentValidation;
using CRM.Application.TaxManagement.Requests;
using System.Text.RegularExpressions;

namespace CRM.Application.TaxManagement.Validators
{
    public class CreateCountryRequestValidator : AbstractValidator<CreateCountryRequest>
    {
        public CreateCountryRequestValidator()
        {
            RuleFor(x => x.CountryName)
                .NotEmpty()
                .WithMessage("Country name is required")
                .MinimumLength(2)
                .WithMessage("Country name must be at least 2 characters")
                .MaximumLength(100)
                .WithMessage("Country name cannot exceed 100 characters");

            RuleFor(x => x.CountryCode)
                .NotEmpty()
                .WithMessage("Country code is required")
                .Length(2)
                .WithMessage("Country code must be exactly 2 characters")
                .Matches(new Regex("^[A-Z]{2}$"))
                .WithMessage("Country code must be 2 uppercase letters (ISO 3166-1 alpha-2)");

            RuleFor(x => x.DefaultCurrency)
                .NotEmpty()
                .WithMessage("Default currency is required")
                .Length(3)
                .WithMessage("Default currency must be exactly 3 characters")
                .Matches(new Regex("^[A-Z]{3}$"))
                .WithMessage("Default currency must be 3 uppercase letters (ISO 4217)");
        }
    }
}

