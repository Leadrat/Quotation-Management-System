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
    public class RejectRefundCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<RejectRefundCommandHandler> _logger;

        public RejectRefundCommandHandler(
            IAppDbContext db,
            ILogger<RejectRefundCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<RefundDto> Handle(RejectRefundCommand command)
        {
            var refund = await _db.Refunds
                .FirstOrDefaultAsync(r => r.RefundId == command.RefundId);

            if (refund == null)
                throw new InvalidOperationException("Refund not found");

            if (!refund.CanBeRejected())
                throw new InvalidOperationException("Refund cannot be rejected in current status");

            // TODO: Validate rejector permissions

            refund.MarkAsRejected(command.Request.RejectionReason);
            if (!string.IsNullOrEmpty(command.Request.Comments))
            {
                refund.Comments = command.Request.Comments;
            }

            // Create timeline entry
            var timelineEntry = new RefundTimeline
            {
                TimelineId = Guid.NewGuid(),
                RefundId = refund.RefundId,
                EventType = RefundTimelineEventType.REJECTED,
                ActedByUserId = command.RejectedByUserId,
                Comments = command.Request.Comments,
                EventDate = DateTimeOffset.UtcNow
            };

            _db.RefundTimeline.Add(timelineEntry);

            await _db.SaveChangesAsync();

            // Publish RefundRejected event
            var refundEvent = new RefundRejected
            {
                RefundId = refund.RefundId,
                PaymentId = refund.PaymentId,
                QuotationId = refund.QuotationId,
                RejectionReason = command.Request.RejectionReason,
                RejectedByUserId = command.RejectedByUserId,
                RejectionDate = DateTimeOffset.UtcNow
            };
            // TODO: Publish event via event bus

            _logger.LogInformation("Refund {RefundId} rejected by user {UserId}",
                refund.RefundId, command.RejectedByUserId);

            // Map to DTO
            var requestedByUser = await _db.Users.FindAsync(refund.RequestedByUserId);
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
                FailureReason = refund.FailureReason,
                Comments = refund.Comments,
                RequestDate = refund.RequestDate
            };
        }
    }
}

