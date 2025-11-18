using CRM.Application.Quotations.Queries;
using FluentValidation;

namespace CRM.Application.Quotations.Validators
{
    public class GetQuotationStatusHistoryQueryValidator : AbstractValidator<GetQuotationStatusHistoryQuery>
    {
        public GetQuotationStatusHistoryQueryValidator()
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


