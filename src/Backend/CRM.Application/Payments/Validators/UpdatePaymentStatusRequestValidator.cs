using System.Linq;
using CRM.Application.Payments.Dtos;
using FluentValidation;

namespace CRM.Application.Payments.Validators
{
    public class UpdatePaymentStatusRequestValidator : AbstractValidator<UpdatePaymentStatusRequest>
    {
        public UpdatePaymentStatusRequestValidator()
        {
            RuleFor(x => x.PaymentReference)
                .NotEmpty()
                .WithMessage("Payment reference is required");

            RuleFor(x => x.Status)
                .NotEmpty()
                .Must(status => new[] { "success", "failed", "pending", "succeeded", "failure", "completed" }
                    .Contains(status.ToLowerInvariant()))
                .WithMessage("Status must be one of: success, failed, pending");

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

