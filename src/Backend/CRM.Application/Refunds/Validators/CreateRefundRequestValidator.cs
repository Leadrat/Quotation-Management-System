using CRM.Application.Refunds.Dtos;
using FluentValidation;

namespace CRM.Application.Refunds.Validators
{
    public class CreateRefundRequestValidator : AbstractValidator<CreateRefundRequest>
    {
        public CreateRefundRequestValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty().WithMessage("Payment ID is required");

            RuleFor(x => x.RefundAmount)
                .GreaterThan(0).When(x => x.RefundAmount.HasValue)
                .WithMessage("Refund amount must be greater than 0");

            RuleFor(x => x.RefundReason)
                .NotEmpty().WithMessage("Refund reason is required")
                .MaximumLength(500).WithMessage("Refund reason must not exceed 500 characters");

            RuleFor(x => x.RefundReasonCode)
                .IsInEnum().WithMessage("Invalid refund reason code");

            RuleFor(x => x.Comments)
                .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Comments))
                .WithMessage("Comments must not exceed 1000 characters");
        }
    }
}

