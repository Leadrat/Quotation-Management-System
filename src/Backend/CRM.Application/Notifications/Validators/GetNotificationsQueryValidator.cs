using FluentValidation;
using CRM.Application.Notifications.Queries;

namespace CRM.Application.Notifications.Validators
{
    public class GetNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
    {
        public GetNotificationsQueryValidator()
        {
            RuleFor(x => x.RequestorUserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.DateTo)
                .GreaterThanOrEqualTo(x => x.DateFrom)
                .WithMessage("DateTo must be greater than or equal to DateFrom")
                .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);
        }
    }
}

