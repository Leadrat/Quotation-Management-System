using FluentValidation;
using CRM.Application.Reports.Queries;

namespace CRM.Application.Reports.Validators
{
    public class GetPaymentAnalyticsQueryValidator : AbstractValidator<GetPaymentAnalyticsQuery>
    {
        public GetPaymentAnalyticsQueryValidator()
        {
            // No required fields - all are optional
        }
    }
}

