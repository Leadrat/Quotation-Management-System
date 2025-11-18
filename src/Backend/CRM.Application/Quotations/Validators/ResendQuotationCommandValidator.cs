using CRM.Application.Quotations.Commands;
using FluentValidation;

namespace CRM.Application.Quotations.Validators
{
    public class ResendQuotationCommandValidator : AbstractValidator<ResendQuotationCommand>
    {
        public ResendQuotationCommandValidator()
        {
            RuleFor(x => x.QuotationId)
                .NotEmpty();

            RuleFor(x => x.RequestedByUserId)
                .NotEmpty();

            RuleFor(x => x.Request)
                .NotNull();
        }
    }
}


