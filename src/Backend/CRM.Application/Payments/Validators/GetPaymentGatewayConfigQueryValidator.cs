using CRM.Application.Payments.Queries;
using FluentValidation;

namespace CRM.Application.Payments.Validators
{
    public class GetPaymentGatewayConfigQueryValidator : AbstractValidator<GetPaymentGatewayConfigQuery>
    {
        public GetPaymentGatewayConfigQueryValidator()
        {
            // CompanyId is optional - if null, returns all configs (admin only)
        }
    }
}

