using CRM.Application.Refunds.Dtos;
using FluentValidation;

namespace CRM.Application.Refunds.Validators
{
    public class ApproveAdjustmentRequestValidator : AbstractValidator<ApproveAdjustmentRequest>
    {
        public ApproveAdjustmentRequestValidator()
        {
            RuleFor(x => x.Comments)
                .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Comments))
                .WithMessage("Comments must not exceed 1000 characters");
        }
    }
}

