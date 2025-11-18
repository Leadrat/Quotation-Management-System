using CRM.Application.Quotations.Dtos;
using FluentValidation;

namespace CRM.Application.Quotations.Validators
{
    public class SubmitQuotationResponseRequestValidator : AbstractValidator<SubmitQuotationResponseRequest>
    {
        private static readonly string[] AllowedResponses = { "ACCEPTED", "REJECTED", "NEEDS_MODIFICATION" };

        public SubmitQuotationResponseRequestValidator()
        {
            RuleFor(x => x.ResponseType)
                .NotEmpty()
                .Must(value => AllowedResponses.Contains(value?.ToUpperInvariant() ?? string.Empty))
                .WithMessage("Response type must be ACCEPTED, REJECTED, or NEEDS_MODIFICATION.");

            RuleFor(x => x.ClientEmail)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.ClientEmail));

            RuleFor(x => x.ResponseMessage)
                .MaximumLength(2000);

            RuleFor(x => x.ClientName)
                .MaximumLength(255);
        }
    }
}


