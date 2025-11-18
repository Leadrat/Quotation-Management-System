using CRM.Application.Payments.Queries;
using FluentValidation;

namespace CRM.Application.Payments.Validators
{
    public class GetPaymentByQuotationQueryValidator : AbstractValidator<GetPaymentByQuotationQuery>
    {
        public GetPaymentByQuotationQueryValidator()
        {
            RuleFor(x => x.QuotationId)
                .NotEmpty()
                .WithMessage("Quotation ID is required");
        }
    }
}

