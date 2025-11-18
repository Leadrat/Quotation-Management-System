using CRM.Application.Refunds.Dtos;
using FluentValidation;

namespace CRM.Application.Refunds.Validators
{
    public class CreateAdjustmentRequestValidator : AbstractValidator<CreateAdjustmentRequest>
    {
        public CreateAdjustmentRequestValidator()
        {
            RuleFor(x => x.QuotationId)
                .NotEmpty().WithMessage("Quotation ID is required");

            RuleFor(x => x.AdjustmentType)
                .IsInEnum().WithMessage("Invalid adjustment type");

            RuleFor(x => x.OriginalAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Original amount must be >= 0");

            RuleFor(x => x.AdjustedAmount)
                .GreaterThan(0).WithMessage("Adjusted amount must be greater than 0");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters");
        }
    }
}

