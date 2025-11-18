using FluentValidation;
using CRM.Application.Reports.Queries;

namespace CRM.Application.Reports.Validators
{
    public class GetClientEngagementMetricsQueryValidator : AbstractValidator<GetClientEngagementMetricsQuery>
    {
        public GetClientEngagementMetricsQueryValidator()
        {
            // No required fields - all are optional
        }
    }
}

