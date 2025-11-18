using FluentValidation;
using CRM.Application.DiscountApprovals.Queries;

namespace CRM.Application.DiscountApprovals.Validators
{
    public class GetQuotationApprovalsQueryValidator : AbstractValidator<GetQuotationApprovalsQuery>
    {
        public GetQuotationApprovalsQueryValidator()
        {
            RuleFor(x => x.QuotationId)
                .NotEmpty()
                .WithMessage("Quotation ID is required.");
        }
    }
}

