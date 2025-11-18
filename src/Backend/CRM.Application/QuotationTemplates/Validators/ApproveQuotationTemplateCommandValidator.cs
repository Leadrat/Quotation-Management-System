using FluentValidation;
using CRM.Application.QuotationTemplates.Commands;

namespace CRM.Application.QuotationTemplates.Validators
{
    public class ApproveQuotationTemplateCommandValidator : AbstractValidator<ApproveQuotationTemplateCommand>
    {
        public ApproveQuotationTemplateCommandValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty().WithMessage("Template ID is required.");

            RuleFor(x => x.ApprovedByUserId)
                .NotEmpty().WithMessage("Approver user ID is required.");
        }
    }
}

