using System;
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
    public class InitiateAdjustmentCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<InitiateAdjustmentCommandHandler> _logger;

        public InitiateAdjustmentCommandHandler(
            IAppDbContext db,
            ILogger<InitiateAdjustmentCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<AdjustmentDto> Handle(InitiateAdjustmentCommand command)
        {
            var request = command.Request;

            // Validate quotation exists
            var quotation = await _db.Quotations
                .FirstOrDefaultAsync(q => q.QuotationId == request.QuotationId);

            if (quotation == null)
                throw new InvalidOperationException("Quotation not found");

            // Calculate tax impact based on adjustment type
            decimal taxImpact = 0;
            if (request.AdjustmentType == AdjustmentType.AMOUNT_CORRECTION || 
                request.AdjustmentType == AdjustmentType.DISCOUNT_CHANGE)
            {
                // Recalculate tax based on new amount
                var taxRate = quotation.SubTotal > 0 ? quotation.TaxAmount / quotation.SubTotal : 0;
                taxImpact = (request.AdjustedAmount - request.OriginalAmount) * taxRate;
            }
            else if (request.AdjustmentType == AdjustmentType.TAX_CORRECTION)
            {
                taxImpact = request.AdjustedAmount - request.OriginalAmount;
            }

            // Determine approval level
            var approvalLevel = DetermineApprovalLevel(Math.Abs(request.AdjustedAmount - request.OriginalAmount));

            // Create adjustment entity
            var adjustment = new Adjustment
            {
                AdjustmentId = Guid.NewGuid(),
                QuotationId = request.QuotationId,
                AdjustmentType = request.AdjustmentType,
                OriginalAmount = request.OriginalAmount,
                AdjustedAmount = request.AdjustedAmount,
                Reason = request.Reason,
                RequestedByUserId = command.RequestedByUserId,
                Status = AdjustmentStatus.PENDING,
                ApprovalLevel = approvalLevel,
                RequestDate = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _db.Adjustments.Add(adjustment);
            await _db.SaveChangesAsync();

            // Publish AdjustmentRequested event
            var adjustmentEvent = new AdjustmentRequested
            {
                AdjustmentId = adjustment.AdjustmentId,
                QuotationId = adjustment.QuotationId,
                AdjustmentType = adjustment.AdjustmentType.ToString(),
                OriginalAmount = adjustment.OriginalAmount,
                AdjustedAmount = adjustment.AdjustedAmount,
                Reason = adjustment.Reason,
                RequestedByUserId = adjustment.RequestedByUserId,
                ApprovalLevel = approvalLevel,
                RequestDate = adjustment.RequestDate
            };
            // TODO: Publish event via event bus

            _logger.LogInformation("Adjustment {AdjustmentId} requested for quotation {QuotationId}",
                adjustment.AdjustmentId, quotation.QuotationId);

            // Map to DTO
            var requestedByUser = await _db.Users.FindAsync(command.RequestedByUserId);
            return new AdjustmentDto
            {
                AdjustmentId = adjustment.AdjustmentId,
                QuotationId = adjustment.QuotationId,
                AdjustmentType = adjustment.AdjustmentType,
                OriginalAmount = adjustment.OriginalAmount,
                AdjustedAmount = adjustment.AdjustedAmount,
                Reason = adjustment.Reason,
                RequestedByUserName = requestedByUser != null ? $"{requestedByUser.FirstName} {requestedByUser.LastName}" : string.Empty,
                Status = adjustment.Status,
                ApprovalLevel = adjustment.ApprovalLevel,
                RequestDate = adjustment.RequestDate,
                AdjustmentDifference = adjustment.GetAdjustmentDifference()
            };
        }

        private string DetermineApprovalLevel(decimal amountDifference)
        {
            // Configurable thresholds
            if (Math.Abs(amountDifference) >= 100000)
                return "Admin";
            if (Math.Abs(amountDifference) >= 50000)
                return "Manager";
            return "Auto";
        }
    }
}

