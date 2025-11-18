using CRM.Application.Clients.Commands;
using CRM.Application.Common.Validation;
using FluentValidation;
using System.Linq;

namespace CRM.Application.Clients.Validators
{
    public class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
    {
        public UpdateClientRequestValidator()
        {
            RuleFor(x => x.CompanyName)
                .MinimumLength(2).When(x => !string.IsNullOrWhiteSpace(x.CompanyName))
                .MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.CompanyName));

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
                .MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Mobile)
                .Matches(@"^\+[1-9]\d{1,14}$").When(x => !string.IsNullOrWhiteSpace(x.Mobile));

            RuleFor(x => x.ContactName)
                .MinimumLength(2).When(x => !string.IsNullOrWhiteSpace(x.ContactName))
                .MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.ContactName))
                .Matches(@"^[a-zA-Z\s'-]{2,255}$").When(x => !string.IsNullOrWhiteSpace(x.ContactName));

            RuleFor(x => x.Gstin)
                .Matches(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$").When(x => !string.IsNullOrWhiteSpace(x.Gstin));

            RuleFor(x => x.StateCode)
                .Matches(@"^[0-9]{2}$").When(x => !string.IsNullOrWhiteSpace(x.StateCode))
                .Must(code => StateCodeConstants.Codes.Contains(code!)).When(x => !string.IsNullOrWhiteSpace(x.StateCode))
                .WithMessage("Invalid state code");

            RuleFor(x => x.PinCode)
                .Matches(@"^[0-9]{6}$").When(x => !string.IsNullOrWhiteSpace(x.PinCode));
        }
    }
}
