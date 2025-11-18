using FluentValidation;
using CRM.Application.Reports.Queries;

namespace CRM.Application.Reports.Validators
{
    public class GetSalesDashboardMetricsQueryValidator : AbstractValidator<GetSalesDashboardMetricsQuery>
    {
        public GetSalesDashboardMetricsQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required");
        }
    }
}

