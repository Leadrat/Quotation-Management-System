using FluentValidation;
using CRM.Application.DiscountApprovals.Queries;

namespace CRM.Application.DiscountApprovals.Validators
{
    public class GetApprovalByIdQueryValidator : AbstractValidator<GetApprovalByIdQuery>
    {
        public GetApprovalByIdQueryValidator()
        {
            RuleFor(x => x.ApprovalId)
                .NotEmpty()
                .WithMessage("Approval ID is required.");

            RuleFor(x => x.RequestorUserId)
                .NotEmpty()
                .WithMessage("Requestor user ID is required.");
        }
    }
}

