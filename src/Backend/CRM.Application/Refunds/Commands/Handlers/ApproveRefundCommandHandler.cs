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
    public class ApproveRefundCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<ApproveRefundCommandHandler> _logger;

        public ApproveRefundCommandHandler(
            IAppDbContext db,
            ILogger<ApproveRefundCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<RefundDto> Handle(ApproveRefundCommand command)
        {
            var refund = await _db.Refunds
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.RefundId == command.RefundId);

            if (refund == null)
                throw new InvalidOperationException("Refund not found");

            if (!refund.CanBeApproved())
                throw new InvalidOperationException("Refund cannot be approved in current status");

            // TODO: Validate approver permissions based on approval level

            var approvalDate = DateTimeOffset.UtcNow;
            refund.MarkAsApproved(command.ApprovedByUserId, approvalDate);
            if (!string.IsNullOrEmpty(command.Request.Comments))
            {
                refund.Comments = command.Request.Comments;
            }

            // Create timeline entry
            var timelineEntry = new RefundTimeline
            {
                TimelineId = Guid.NewGuid(),
                RefundId = refund.RefundId,
                EventType = RefundTimelineEventType.APPROVED,
                ActedByUserId = command.ApprovedByUserId,
                Comments = command.Request.Comments,
                EventDate = approvalDate
            };

            _db.RefundTimeline.Add(timelineEntry);

            await _db.SaveChangesAsync();

            // Publish RefundApproved event
            var refundEvent = new RefundApproved
            {
                RefundId = refund.RefundId,
                PaymentId = refund.PaymentId,
                QuotationId = refund.QuotationId,
                RefundAmount = refund.RefundAmount,
                RequestedByUserId = refund.RequestedByUserId,
                ApprovedByUserId = command.ApprovedByUserId,
                ApprovalDate = approvalDate
            };
            // TODO: Publish event via event bus

            _logger.LogInformation("Refund {RefundId} approved by user {UserId}",
                refund.RefundId, command.ApprovedByUserId);

            // Map to DTO
            var requestedByUser = await _db.Users.FindAsync(refund.RequestedByUserId);
            var approvedByUser = await _db.Users.FindAsync(command.ApprovedByUserId);
            return new RefundDto
            {
                RefundId = refund.RefundId,
                PaymentId = refund.PaymentId,
                QuotationId = refund.QuotationId,
                RefundAmount = refund.RefundAmount,
                RefundReason = refund.RefundReason,
                RefundReasonCode = refund.RefundReasonCode,
                RequestedByUserName = requestedByUser != null ? $"{requestedByUser.FirstName} {requestedByUser.LastName}" : string.Empty,
                ApprovedByUserName = approvedByUser != null ? $"{approvedByUser.FirstName} {approvedByUser.LastName}" : string.Empty,
                RefundStatus = refund.RefundStatus,
                ApprovalLevel = refund.ApprovalLevel,
                Comments = refund.Comments,
                RequestDate = refund.RequestDate,
                ApprovalDate = refund.ApprovalDate
            };
        }
    }
}

