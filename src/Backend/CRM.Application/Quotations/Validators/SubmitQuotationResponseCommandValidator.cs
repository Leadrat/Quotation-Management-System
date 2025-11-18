using CRM.Application.Quotations.Commands;
using FluentValidation;

namespace CRM.Application.Quotations.Validators
{
    public class SubmitQuotationResponseCommandValidator : AbstractValidator<SubmitQuotationResponseCommand>
    {
        public SubmitQuotationResponseCommandValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty();

            RuleFor(x => x.Request)
                .NotNull();
        }
    }
}


