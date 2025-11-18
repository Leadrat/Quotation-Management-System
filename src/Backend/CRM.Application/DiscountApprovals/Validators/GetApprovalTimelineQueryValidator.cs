using FluentValidation;
using CRM.Application.DiscountApprovals.Queries;

namespace CRM.Application.DiscountApprovals.Validators
{
    public class GetApprovalTimelineQueryValidator : AbstractValidator<GetApprovalTimelineQuery>
    {
        public GetApprovalTimelineQueryValidator()
        {
            RuleFor(x => x)
                .Must(x => x.ApprovalId.HasValue || x.QuotationId.HasValue)
                .WithMessage("Either ApprovalId or QuotationId must be provided.")
                .Must(x => !(x.ApprovalId.HasValue && x.QuotationId.HasValue))
                .WithMessage("Cannot provide both ApprovalId and QuotationId.");
        }
    }
}

