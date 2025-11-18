using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using CRM.Application.Common.Persistence;
using CRM.Application.DiscountApprovals.Commands;
using CRM.Application.DiscountApprovals.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.DiscountApprovals.Validators
{
    public class RequestDiscountApprovalCommandValidator : AbstractValidator<RequestDiscountApprovalCommand>
    {
        private readonly IAppDbContext _db;

        public RequestDiscountApprovalCommandValidator(IAppDbContext db)
        {
            _db = db;

            RuleFor(x => x.Request.QuotationId)
                .NotEmpty()
                .WithMessage("Quotation ID is required.")
                .MustAsync(QuotationExistsAsync)
                .WithMessage("Quotation not found.")
                .MustAsync(QuotationNotPendingAsync)
                .WithMessage("Quotation is already pending approval.");

            RuleFor(x => x.Request.DiscountPercentage)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Discount percentage must be between 0 and 100.");

            RuleFor(x => x.Request.Reason)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(2000)
                .WithMessage("Reason is required and must be between 10 and 2000 characters.");

            RuleFor(x => x.Request.Comments)
                .MaximumLength(5000)
                .When(x => !string.IsNullOrEmpty(x.Request.Comments))
                .WithMessage("Comments cannot exceed 5000 characters.");

            RuleFor(x => x.RequestedByUserId)
                .NotEmpty()
                .WithMessage("Requested by user ID is required.");
        }

        private async Task<bool> QuotationExistsAsync(Guid quotationId, CancellationToken cancellationToken)
        {
            return await _db.Quotations.AnyAsync(q => q.QuotationId == quotationId, cancellationToken);
        }

        private async Task<bool> QuotationNotPendingAsync(Guid quotationId, CancellationToken cancellationToken)
        {
            var quotation = await _db.Quotations
                .FirstOrDefaultAsync(q => q.QuotationId == quotationId, cancellationToken);
            return quotation != null && !quotation.IsPendingApproval;
        }
    }
}

