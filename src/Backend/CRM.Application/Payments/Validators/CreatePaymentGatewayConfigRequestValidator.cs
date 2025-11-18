using CRM.Application.Payments.Dtos;
using FluentValidation;

namespace CRM.Application.Payments.Validators
{
    public class CreatePaymentGatewayConfigRequestValidator : AbstractValidator<CreatePaymentGatewayConfigRequest>
    {
        public CreatePaymentGatewayConfigRequestValidator()
        {
            RuleFor(x => x.GatewayName)
                .NotEmpty()
                .MaximumLength(50)
                .Must(name => new[] { "Stripe", "Razorpay", "PayPal" }.Contains(name))
                .WithMessage("Gateway name must be one of: Stripe, Razorpay, PayPal");

            RuleFor(x => x.ApiKey)
                .NotEmpty()
                .WithMessage("API key is required");

            RuleFor(x => x.ApiSecret)
                .NotEmpty()
                .WithMessage("API secret is required");
        }
    }
}

