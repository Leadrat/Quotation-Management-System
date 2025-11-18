using FluentValidation;
using CRM.Application.QuotationTemplates.Commands;

namespace CRM.Application.QuotationTemplates.Validators
{
    public class ApplyTemplateToQuotationCommandValidator : AbstractValidator<ApplyTemplateToQuotationCommand>
    {
        public ApplyTemplateToQuotationCommandValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty().WithMessage("Template ID is required.");

            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("Client ID is required.");

            RuleFor(x => x.AppliedByUserId)
                .NotEmpty().WithMessage("Applied by user ID is required.");
        }
    }
}

