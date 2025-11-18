using FluentValidation;
using CRM.Application.DiscountApprovals.Commands;

namespace CRM.Application.DiscountApprovals.Validators
{
    public class ResubmitDiscountApprovalCommandValidator : AbstractValidator<ResubmitDiscountApprovalCommand>
    {
        public ResubmitDiscountApprovalCommandValidator()
        {
            RuleFor(x => x.ApprovalId)
                .NotEmpty()
                .WithMessage("Approval ID is required.");

            RuleFor(x => x.Request.Reason)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(2000)
                .WithMessage("Reason is required and must be between 10 and 2000 characters.");

            RuleFor(x => x.Request.Comments)
                .MaximumLength(5000)
                .When(x => !string.IsNullOrEmpty(x.Request.Comments))
                .WithMessage("Comments cannot exceed 5000 characters.");

            RuleFor(x => x.ResubmittedByUserId)
                .NotEmpty()
                .WithMessage("Resubmitted by user ID is required.");
        }
    }
}

