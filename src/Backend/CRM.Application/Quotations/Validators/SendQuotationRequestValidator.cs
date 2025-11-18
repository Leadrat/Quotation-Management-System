using System.Linq;
using CRM.Application.Quotations.Dtos;
using FluentValidation;

namespace CRM.Application.Quotations.Validators
{
    public class SendQuotationRequestValidator : AbstractValidator<SendQuotationRequest>
    {
        public SendQuotationRequestValidator()
        {
            RuleFor(x => x.RecipientEmail)
                .NotEmpty().WithMessage("Recipient email is required.")
                .EmailAddress().WithMessage("Recipient email must be valid.");

            When(x => x.CcEmails != null && x.CcEmails.Any(), () =>
            {
                RuleForEach(x => x.CcEmails!)
                    .EmailAddress().WithMessage("CC email must be valid.");
            });

            When(x => x.BccEmails != null && x.BccEmails.Any(), () =>
            {
                RuleForEach(x => x.BccEmails!)
                    .EmailAddress().WithMessage("BCC email must be valid.");
            });

            RuleFor(x => x.CustomMessage)
                .MaximumLength(2000)
                .WithMessage("Custom message cannot exceed 2000 characters.");
        }
    }
}


