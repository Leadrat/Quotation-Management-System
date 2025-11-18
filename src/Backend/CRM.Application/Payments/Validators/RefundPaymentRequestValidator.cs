using CRM.Application.Payments.Dtos;
using FluentValidation;

namespace CRM.Application.Payments.Validators
{
    public class RefundPaymentRequestValidator : AbstractValidator<RefundPaymentRequest>
    {
        public RefundPaymentRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .When(x => x.Amount.HasValue)
                .WithMessage("Refund amount must be greater than zero");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .MaximumLength(500)
                .WithMessage("Refund reason is required and must not exceed 500 characters");
        }
    }
}

