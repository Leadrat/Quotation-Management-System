using FluentValidation;
using CRM.Application.Reports.Queries;

namespace CRM.Application.Reports.Validators
{
    public class GetDiscountAnalyticsQueryValidator : AbstractValidator<GetDiscountAnalyticsQuery>
    {
        public GetDiscountAnalyticsQueryValidator()
        {
            // No required fields - all are optional
        }
    }
}

