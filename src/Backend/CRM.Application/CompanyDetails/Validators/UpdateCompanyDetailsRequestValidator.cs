using CRM.Application.CompanyDetails.Dtos;
using FluentValidation;

namespace CRM.Application.CompanyDetails.Validators
{
    public class UpdateCompanyDetailsRequestValidator : AbstractValidator<UpdateCompanyDetailsRequest>
    {
        public UpdateCompanyDetailsRequestValidator()
        {
            RuleFor(x => x.PanNumber)
                .Matches(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$")
                .WithMessage("PAN number must be in format: ABCDE1234F (5 letters, 4 digits, 1 letter)")
                .When(x => !string.IsNullOrWhiteSpace(x.PanNumber));

            RuleFor(x => x.TanNumber)
                .Matches(@"^[A-Z]{4}[0-9]{5}[A-Z]{1}$")
                .WithMessage("TAN number must be in format: ABCD12345E (4 letters, 5 digits, 1 letter)")
                .When(x => !string.IsNullOrWhiteSpace(x.TanNumber));

            RuleFor(x => x.GstNumber)
                .Matches(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$")
                .WithMessage("GST number must be in format: 27ABCDE1234F1Z5 (15 characters)")
                .When(x => !string.IsNullOrWhiteSpace(x.GstNumber));

            RuleFor(x => x.ContactEmail)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

            RuleFor(x => x.Website)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.Website))
                .WithMessage("Website must be a valid URL");

            RuleForEach(x => x.BankDetails)
                .SetValidator(new BankDetailsDtoValidator());
        }
    }

    public class BankDetailsDtoValidator : AbstractValidator<BankDetailsDto>
    {
        public BankDetailsDtoValidator()
        {
            RuleFor(x => x.Country)
                .NotEmpty()
                .Must(c => c == "India" || c == "Dubai")
                .WithMessage("Country must be 'India' or 'Dubai'");

            RuleFor(x => x.AccountNumber)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.BankName)
                .NotEmpty()
                .MaximumLength(255);

            // India-specific validation
            When(x => x.Country == "India", () =>
            {
                RuleFor(x => x.IfscCode)
                    .NotEmpty()
                    .Matches(@"^[A-Z]{4}0[A-Z0-9]{6}$")
                    .WithMessage("IFSC code must be 11 characters: 4 letters, 1 zero, 6 alphanumeric");
            });

            // Dubai-specific validation
            When(x => x.Country == "Dubai", () =>
            {
                RuleFor(x => x.Iban)
                    .NotEmpty()
                    .MinimumLength(15)
                    .MaximumLength(34);
                
                RuleFor(x => x.SwiftCode)
                    .NotEmpty()
                    .Matches(@"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$")
                    .WithMessage("SWIFT code must be 8-11 characters");
            });
        }
    }
}

