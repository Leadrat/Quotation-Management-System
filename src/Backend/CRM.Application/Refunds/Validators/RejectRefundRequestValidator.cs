using CRM.Application.Refunds.Dtos;
using FluentValidation;

namespace CRM.Application.Refunds.Validators
{
    public class RejectRefundRequestValidator : AbstractValidator<RejectRefundRequest>
    {
        public RejectRefundRequestValidator()
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required")
                .MaximumLength(500).WithMessage("Rejection reason must not exceed 500 characters");

            RuleFor(x => x.Comments)
                .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Comments))
                .WithMessage("Comments must not exceed 1000 characters");
        }
    }
}

