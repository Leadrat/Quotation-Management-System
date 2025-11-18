using CRM.Application.Quotations.Commands;
using FluentValidation;

namespace CRM.Application.Quotations.Validators
{
    public class MarkQuotationAsExpiredCommandValidator : AbstractValidator<MarkQuotationAsExpiredCommand>
    {
        public MarkQuotationAsExpiredCommandValidator()
        {
            RuleFor(x => x.QuotationId)
                .NotEmpty();

            RuleFor(x => x.Reason)
                .MaximumLength(500);
        }
    }
}


