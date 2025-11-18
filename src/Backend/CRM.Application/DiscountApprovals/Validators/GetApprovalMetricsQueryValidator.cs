using FluentValidation;
using CRM.Application.DiscountApprovals.Queries;

namespace CRM.Application.DiscountApprovals.Validators
{
    public class GetApprovalMetricsQueryValidator : AbstractValidator<GetApprovalMetricsQuery>
    {
        public GetApprovalMetricsQueryValidator()
        {
            RuleFor(x => x.DateFrom)
                .LessThanOrEqualTo(x => x.DateTo)
                .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
                .WithMessage("Date from must be less than or equal to date to.");
        }
    }
}

