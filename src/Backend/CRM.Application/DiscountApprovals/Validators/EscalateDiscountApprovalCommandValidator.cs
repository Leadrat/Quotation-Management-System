using FluentValidation;
using CRM.Application.DiscountApprovals.Commands;

namespace CRM.Application.DiscountApprovals.Validators
{
    public class EscalateDiscountApprovalCommandValidator : AbstractValidator<EscalateDiscountApprovalCommand>
    {
        public EscalateDiscountApprovalCommandValidator()
        {
            RuleFor(x => x.ApprovalId)
                .NotEmpty()
                .WithMessage("Approval ID is required.");

            RuleFor(x => x.EscalatedByUserId)
                .NotEmpty()
                .WithMessage("Escalated by user ID is required.");

            RuleFor(x => x.Reason)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrEmpty(x.Reason))
                .WithMessage("Reason cannot exceed 2000 characters.");
        }
    }
}

