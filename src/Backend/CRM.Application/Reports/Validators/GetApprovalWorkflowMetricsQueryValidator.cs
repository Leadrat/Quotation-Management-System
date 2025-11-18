using FluentValidation;
using CRM.Application.Reports.Queries;

namespace CRM.Application.Reports.Validators
{
    public class GetApprovalWorkflowMetricsQueryValidator : AbstractValidator<GetApprovalWorkflowMetricsQuery>
    {
        public GetApprovalWorkflowMetricsQueryValidator()
        {
            // No required fields - all are optional
        }
    }
}

