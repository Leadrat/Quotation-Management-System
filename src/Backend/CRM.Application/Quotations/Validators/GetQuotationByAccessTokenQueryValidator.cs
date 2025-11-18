using CRM.Application.Quotations.Queries;
using FluentValidation;

namespace CRM.Application.Quotations.Validators
{
    public class GetQuotationByAccessTokenQueryValidator : AbstractValidator<GetQuotationByAccessTokenQuery>
    {
        public GetQuotationByAccessTokenQueryValidator()
        {
            RuleFor(x => x.QuotationId)
                .NotEmpty();

            RuleFor(x => x.AccessToken)
                .NotEmpty();
        }
    }
}


