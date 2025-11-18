using CRM.Application.Payments.Queries;
using FluentValidation;

namespace CRM.Application.Payments.Validators
{
    public class GetPaymentsDashboardQueryValidator : AbstractValidator<GetPaymentsDashboardQuery>
    {
        public GetPaymentsDashboardQueryValidator()
        {
            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("End date must be after start date");
        }
    }
}

