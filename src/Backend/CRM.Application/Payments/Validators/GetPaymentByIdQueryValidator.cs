using CRM.Application.Payments.Queries;
using FluentValidation;

namespace CRM.Application.Payments.Validators
{
    public class GetPaymentByIdQueryValidator : AbstractValidator<GetPaymentByIdQuery>
    {
        public GetPaymentByIdQueryValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty()
                .WithMessage("Payment ID is required");
        }
    }
}

