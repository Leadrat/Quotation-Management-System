using CRM.Application.Payments.Dtos;
using FluentValidation;

namespace CRM.Application.Payments.Validators
{
    public class InitiatePaymentRequestValidator : AbstractValidator<InitiatePaymentRequest>
    {
        public InitiatePaymentRequestValidator()
        {
            RuleFor(x => x.QuotationId)
                .NotEmpty()
                .WithMessage("Quotation ID is required");

            RuleFor(x => x.PaymentGateway)
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("Payment gateway is required and must not exceed 50 characters");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .When(x => x.Amount.HasValue)
                .WithMessage("Amount must be greater than zero");

            RuleFor(x => x.Currency)
                .MaximumLength(3)
                .When(x => !string.IsNullOrEmpty(x.Currency))
                .WithMessage("Currency code must not exceed 3 characters");
        }
    }
}

