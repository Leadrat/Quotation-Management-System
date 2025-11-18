using FluentValidation;
using CRM.Application.DiscountApprovals.Commands;

namespace CRM.Application.DiscountApprovals.Validators
{
    public class BulkApproveDiscountApprovalsCommandValidator : AbstractValidator<BulkApproveDiscountApprovalsCommand>
    {
        public BulkApproveDiscountApprovalsCommandValidator()
        {
            RuleFor(x => x.Request.ApprovalIds)
                .NotEmpty()
                .WithMessage("At least one approval ID is required.")
                .Must(ids => ids.Count <= 50)
                .WithMessage("Cannot approve more than 50 approvals at once.");

            RuleFor(x => x.Request.Reason)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(2000)
                .WithMessage("Reason is required and must be between 10 and 2000 characters.");

            RuleFor(x => x.Request.Comments)
                .MaximumLength(5000)
                .When(x => !string.IsNullOrEmpty(x.Request.Comments))
                .WithMessage("Comments cannot exceed 5000 characters.");

            RuleFor(x => x.ApprovedByUserId)
                .NotEmpty()
                .WithMessage("Approved by user ID is required.");
        }
    }
}

