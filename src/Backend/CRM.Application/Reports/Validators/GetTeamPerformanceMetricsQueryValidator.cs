using FluentValidation;
using CRM.Application.Reports.Queries;

namespace CRM.Application.Reports.Validators
{
    public class GetTeamPerformanceMetricsQueryValidator : AbstractValidator<GetTeamPerformanceMetricsQuery>
    {
        public GetTeamPerformanceMetricsQueryValidator()
        {
            // No required fields - all are optional
        }
    }
}

