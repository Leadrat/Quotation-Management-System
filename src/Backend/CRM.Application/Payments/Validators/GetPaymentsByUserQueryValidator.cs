using CRM.Application.Payments.Queries;
using FluentValidation;

namespace CRM.Application.Payments.Validators
{
    public class GetPaymentsByUserQueryValidator : AbstractValidator<GetPaymentsByUserQuery>
    {
        public GetPaymentsByUserQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than zero");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("End date must be after start date");
        }
    }
}

