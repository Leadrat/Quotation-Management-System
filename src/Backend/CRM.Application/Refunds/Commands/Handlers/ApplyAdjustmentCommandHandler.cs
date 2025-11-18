using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Refunds.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.Commands.Handlers
{
    public class ApplyAdjustmentCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<ApplyAdjustmentCommandHandler> _logger;

        public ApplyAdjustmentCommandHandler(
            IAppDbContext db,
            ILogger<ApplyAdjustmentCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<AdjustmentDto> Handle(ApplyAdjustmentCommand command)
        {
            var adjustment = await _db.Adjustments
                .Include(a => a.Quotation)
                .FirstOrDefaultAsync(a => a.AdjustmentId == command.AdjustmentId);

            if (adjustment == null)
                throw new InvalidOperationException("Adjustment not found");

            if (!adjustment.CanBeApplied())
                throw new InvalidOperationException("Adjustment must be approved before applying");

            var quotation = adjustment.Quotation;
            var appliedDate = DateTimeOffset.UtcNow;

            // Apply adjustment based on type
            if (adjustment.AdjustmentType == AdjustmentType.DISCOUNT_CHANGE)
            {
                // Update discount amount
                var discountDifference = adjustment.AdjustedAmount - adjustment.OriginalAmount;
                quotation.DiscountAmount = quotation.DiscountAmount + discountDifference;
            }
            else if (adjustment.AdjustmentType == AdjustmentType.AMOUNT_CORRECTION)
            {
                // Update subtotal
                var amountDifference = adjustment.AdjustedAmount - adjustment.OriginalAmount;
                quotation.SubTotal = quotation.SubTotal + amountDifference;
            }
            else if (adjustment.AdjustmentType == AdjustmentType.TAX_CORRECTION)
            {
                // Update tax amount
                var taxDifference = adjustment.AdjustedAmount - adjustment.OriginalAmount;
                quotation.TaxAmount = quotation.TaxAmount + taxDifference;
            }

            // Recalculate total
            quotation.TotalAmount = quotation.SubTotal + quotation.TaxAmount - quotation.DiscountAmount;
            quotation.UpdatedAt = DateTimeOffset.UtcNow;

            // Update any related payments if needed
            var payments = await _db.Payments
                .Where(p => p.QuotationId == quotation.QuotationId && p.PaymentStatus == PaymentStatus.Success)
                .ToListAsync();

            // Mark adjustment as applied
            adjustment.MarkAsApplied(appliedDate);

            await _db.SaveChangesAsync();

            // Publish AdjustmentApplied event
            var adjustmentEvent = new AdjustmentApplied
            {
                AdjustmentId = adjustment.AdjustmentId,
                QuotationId = adjustment.QuotationId,
                OriginalAmount = adjustment.OriginalAmount,
                AdjustedAmount = adjustment.AdjustedAmount,
                AppliedDate = appliedDate
            };
            // TODO: Publish event via event bus

            _logger.LogInformation("Adjustment {AdjustmentId} applied to quotation {QuotationId}",
                adjustment.AdjustmentId, quotation.QuotationId);

            // Map to DTO
            var requestedByUser = await _db.Users.FindAsync(adjustment.RequestedByUserId);
            var approvedByUser = adjustment.ApprovedByUserId.HasValue 
                ? await _db.Users.FindAsync(adjustment.ApprovedByUserId.Value) 
                : null;
            return new AdjustmentDto
            {
                AdjustmentId = adjustment.AdjustmentId,
                QuotationId = adjustment.QuotationId,
                AdjustmentType = adjustment.AdjustmentType,
                OriginalAmount = adjustment.OriginalAmount,
                AdjustedAmount = adjustment.AdjustedAmount,
                Reason = adjustment.Reason,
                RequestedByUserName = requestedByUser != null ? $"{requestedByUser.FirstName} {requestedByUser.LastName}" : string.Empty,
                ApprovedByUserName = approvedByUser != null ? $"{approvedByUser.FirstName} {approvedByUser.LastName}" : string.Empty,
                Status = adjustment.Status,
                ApprovalLevel = adjustment.ApprovalLevel,
                RequestDate = adjustment.RequestDate,
                ApprovalDate = adjustment.ApprovalDate,
                AppliedDate = adjustment.AppliedDate,
                AdjustmentDifference = adjustment.GetAdjustmentDifference()
            };
        }
    }
}

