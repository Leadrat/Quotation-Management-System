using CRM.Application.Payments.Dtos;
using FluentValidation;

namespace CRM.Application.Payments.Validators
{
    public class UpdatePaymentGatewayConfigRequestValidator : AbstractValidator<UpdatePaymentGatewayConfigRequest>
    {
        public UpdatePaymentGatewayConfigRequestValidator()
        {
            // All fields are optional for update
            RuleFor(x => x.ApiKey)
                .NotEmpty()
                .When(x => !string.IsNullOrEmpty(x.ApiKey))
                .WithMessage("API key cannot be empty if provided");

            RuleFor(x => x.ApiSecret)
                .NotEmpty()
                .When(x => !string.IsNullOrEmpty(x.ApiSecret))
                .WithMessage("API secret cannot be empty if provided");
        }
    }
}

