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
    public class InitiateRefundCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<InitiateRefundCommandHandler> _logger;

        public InitiateRefundCommandHandler(
            IAppDbContext db,
            ILogger<InitiateRefundCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<RefundDto> Handle(InitiateRefundCommand command)
        {
            var request = command.Request;

            // Validate payment exists
            var payment = await _db.Payments
                .Include(p => p.Quotation)
                .FirstOrDefaultAsync(p => p.PaymentId == request.PaymentId);

            if (payment == null)
                throw new InvalidOperationException("Payment not found");

            if (payment.PaymentStatus != PaymentStatus.Success)
                throw new InvalidOperationException("Refund can only be initiated for successful payments");

            // Get quotation ID if not provided
            var quotationId = request.QuotationId ?? payment.QuotationId;

            // Calculate refund amount (full or partial)
            var existingRefunds = await _db.Refunds
                .Where(r => r.PaymentId == request.PaymentId && 
                           (r.RefundStatus == RefundStatus.Completed || r.RefundStatus == RefundStatus.Processing))
                .SumAsync(r => r.RefundAmount);

            var availableAmount = payment.AmountPaid - existingRefunds;
            var refundAmount = request.RefundAmount ?? availableAmount;

            if (refundAmount <= 0)
                throw new InvalidOperationException("Refund amount must be greater than zero");

            if (refundAmount > availableAmount)
                throw new InvalidOperationException($"Refund amount exceeds available refundable amount. Available: {availableAmount}");

            // Determine approval level based on amount
            var approvalLevel = DetermineApprovalLevel(refundAmount);

            // Create refund entity
            var refund = new Refund
            {
                RefundId = Guid.NewGuid(),
                PaymentId = request.PaymentId,
                QuotationId = quotationId,
                RefundAmount = refundAmount,
                RefundReason = request.RefundReason,
                RefundReasonCode = request.RefundReasonCode,
                RequestedByUserId = command.RequestedByUserId,
                RefundStatus = RefundStatus.Pending,
                ApprovalLevel = approvalLevel,
                Comments = request.Comments,
                RequestDate = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _db.Refunds.Add(refund);

            // Create timeline entry
            var timelineEntry = new RefundTimeline
            {
                TimelineId = Guid.NewGuid(),
                RefundId = refund.RefundId,
                EventType = RefundTimelineEventType.REQUESTED,
                ActedByUserId = command.RequestedByUserId,
                Comments = request.Comments,
                EventDate = DateTimeOffset.UtcNow
            };

            _db.RefundTimeline.Add(timelineEntry);

            await _db.SaveChangesAsync();

            // Publish RefundRequested event
            var refundEvent = new RefundRequested
            {
                RefundId = refund.RefundId,
                PaymentId = refund.PaymentId,
                QuotationId = refund.QuotationId,
                RefundAmount = refund.RefundAmount,
                RefundReason = refund.RefundReason,
                RequestedByUserId = refund.RequestedByUserId,
                ApprovalLevel = approvalLevel,
                RequestDate = refund.RequestDate
            };
            // TODO: Publish event via event bus

            _logger.LogInformation("Refund {RefundId} requested for payment {PaymentId}, amount: {Amount}",
                refund.RefundId, payment.PaymentId, refundAmount);

            // Map to DTO
            var requestedByUser = await _db.Users.FindAsync(command.RequestedByUserId);
            return new RefundDto
            {
                RefundId = refund.RefundId,
                PaymentId = refund.PaymentId,
                QuotationId = refund.QuotationId,
                RefundAmount = refund.RefundAmount,
                RefundReason = refund.RefundReason,
                RefundReasonCode = refund.RefundReasonCode,
                RequestedByUserName = requestedByUser != null ? $"{requestedByUser.FirstName} {requestedByUser.LastName}" : string.Empty,
                RefundStatus = refund.RefundStatus,
                ApprovalLevel = refund.ApprovalLevel,
                Comments = refund.Comments,
                RequestDate = refund.RequestDate
            };
        }

        private string DetermineApprovalLevel(decimal amount)
        {
            // Configurable thresholds - can be moved to settings
            if (amount >= 100000) // >= 1 lakh
                return "Admin";
            if (amount >= 50000) // >= 50k
                return "Manager";
            return "Auto"; // Auto-approve for smaller amounts
        }
    }
}

