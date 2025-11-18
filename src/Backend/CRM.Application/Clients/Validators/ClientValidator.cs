using CRM.Application.Common.Validation;
using FluentValidation;
using System.Linq;

namespace CRM.Application.Clients.Validators
{
    public class ClientValidator : AbstractValidator<CRM.Application.Clients.Commands.CreateClientRequest>
    {
        public ClientValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(255);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(255);

            RuleFor(x => x.Mobile)
                .NotEmpty()
                .Matches(@"^\+[1-9]\d{1,14}$");

            RuleFor(x => x.ContactName)
                .MinimumLength(2).When(x => !string.IsNullOrWhiteSpace(x.ContactName))
                .MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.ContactName))
                .Matches(@"^[a-zA-Z\s'-]{2,255}$").When(x => !string.IsNullOrWhiteSpace(x.ContactName));

            RuleFor(x => x.Gstin)
                .Matches(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$")
                .When(x => !string.IsNullOrWhiteSpace(x.Gstin));

            RuleFor(x => x.StateCode)
                .Matches(@"^[0-9]{2}$").When(x => !string.IsNullOrWhiteSpace(x.StateCode))
                .Must(code => StateCodeConstants.Codes.Contains(code!)).When(x => !string.IsNullOrWhiteSpace(x.StateCode))
                .WithMessage("Invalid state code");

            RuleFor(x => x.PinCode)
                .Matches(@"^[0-9]{6}$").When(x => !string.IsNullOrWhiteSpace(x.PinCode));
        }
    }
}
