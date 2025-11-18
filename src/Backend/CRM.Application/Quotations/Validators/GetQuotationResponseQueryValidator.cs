using CRM.Application.Quotations.Queries;
using FluentValidation;

namespace CRM.Application.Quotations.Validators
{
    public class GetQuotationResponseQueryValidator : AbstractValidator<GetQuotationResponseQuery>
    {
        public GetQuotationResponseQueryValidator()
        {
            RuleFor(x => x.QuotationId)
                .NotEmpty();

            RuleFor(x => x.RequestorUserId)
                .NotEmpty();

            RuleFor(x => x.RequestorRole)
                .NotEmpty();
        }
    }
}


