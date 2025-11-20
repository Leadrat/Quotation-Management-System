using CRM.Application.CompanyBankDetails.DTOs;
using FluentValidation;

namespace CRM.Application.CompanyBankDetails.Validators
{
    public class ConfigureCountryBankFieldRequestValidator : AbstractValidator<ConfigureCountryBankFieldRequest>
    {
        public ConfigureCountryBankFieldRequestValidator()
        {
            RuleFor(x => x.CountryId)
                .NotEmpty();

            RuleFor(x => x.BankFieldTypeId)
                .NotEmpty();

            RuleFor(x => x.ValidationRegex)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.ValidationRegex))
                .Must(BeValidRegex).When(x => !string.IsNullOrWhiteSpace(x.ValidationRegex))
                .WithMessage("ValidationRegex must be a valid regular expression");

            RuleFor(x => x.MinLength)
                .GreaterThan(0).When(x => x.MinLength.HasValue)
                .LessThanOrEqualTo(x => x.MaxLength ?? int.MaxValue).When(x => x.MinLength.HasValue && x.MaxLength.HasValue)
                .WithMessage("MinLength must be less than or equal to MaxLength");

            RuleFor(x => x.MaxLength)
                .GreaterThan(0).When(x => x.MaxLength.HasValue)
                .GreaterThanOrEqualTo(x => x.MinLength ?? 0).When(x => x.MaxLength.HasValue && x.MinLength.HasValue)
                .WithMessage("MaxLength must be greater than or equal to MinLength");

            RuleFor(x => x.DisplayName)
                .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

            RuleFor(x => x.HelpText)
                .MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.HelpText));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0);
        }

        private bool BeValidRegex(string? regex)
        {
            if (string.IsNullOrWhiteSpace(regex))
                return true;

            try
            {
                System.Text.RegularExpressions.Regex.Match("", regex);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

