using FluentValidation;
using CRM.Application.DiscountApprovals.Queries;

namespace CRM.Application.DiscountApprovals.Validators
{
    public class GetPendingApprovalsQueryValidator : AbstractValidator<GetPendingApprovalsQuery>
    {
        public GetPendingApprovalsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be at least 1.");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(100)
                .WithMessage("Page size must be between 1 and 100.");

            RuleFor(x => x.DateFrom)
                .LessThanOrEqualTo(x => x.DateTo)
                .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
                .WithMessage("Date from must be less than or equal to date to.");

            RuleFor(x => x.DiscountPercentageMin)
                .LessThanOrEqualTo(x => x.DiscountPercentageMax)
                .When(x => x.DiscountPercentageMin.HasValue && x.DiscountPercentageMax.HasValue)
                .WithMessage("Minimum discount percentage must be less than or equal to maximum.");

            RuleFor(x => x.RequestorUserId)
                .NotEmpty()
                .WithMessage("Requestor user ID is required.");
        }
    }
}

