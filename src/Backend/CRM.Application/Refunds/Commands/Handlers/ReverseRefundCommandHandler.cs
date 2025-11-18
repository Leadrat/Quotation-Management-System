using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Services;
using CRM.Application.Refunds.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.Commands.Handlers
{
    public class ReverseRefundCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly ILogger<ReverseRefundCommandHandler> _logger;

        public ReverseRefundCommandHandler(
            IAppDbContext db,
            IPaymentGatewayFactory gatewayFactory,
            ILogger<ReverseRefundCommandHandler> logger)
        {
            _db = db;
            _gatewayFactory = gatewayFactory;
            _logger = logger;
        }

        public async Task<RefundDto> Handle(ReverseRefundCommand command)
        {
            var refund = await _db.Refunds
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.RefundId == command.RefundId);

            if (refund == null)
                throw new InvalidOperationException("Refund not found");

            if (!refund.CanBeReversed())
                throw new InvalidOperationException("Refund cannot be reversed in current status");

            // Get gateway service
            var gatewayService = await _gatewayFactory.GetGatewayServiceAsync(refund.Payment.PaymentGateway);
            if (gatewayService == null)
                throw new InvalidOperationException($"Payment gateway '{refund.Payment.PaymentGateway}' not found");

            // Get gateway config
            var gatewayConfig = await _db.PaymentGatewayConfigs
                .FirstOrDefaultAsync(c => c.GatewayName == refund.Payment.PaymentGateway && c.Enabled);

            if (gatewayConfig == null)
                throw new InvalidOperationException($"Payment gateway '{refund.Payment.PaymentGateway}' is not configured");

            // Attempt to reverse via gateway
            if (!string.IsNullOrEmpty(refund.PaymentGatewayReference))
            {
                var reverseResponse = await gatewayService.ReverseRefundAsync(
                    refund.PaymentGatewayReference,
                    command.Request.ReversedReason,
                    gatewayConfig.ApiKey,
                    gatewayConfig.ApiSecret,
                    gatewayConfig.IsTestMode);

                if (!reverseResponse.Success)
                {
                    _logger.LogWarning("Gateway does not support refund reversal: {Error}",
                        reverseResponse.ErrorMessage);
                }
            }

            var reversedDate = DateTimeOffset.UtcNow;
            refund.MarkAsReversed(command.Request.ReversedReason, reversedDate);

            // Create timeline entry
            var timelineEntry = new RefundTimeline
            {
                TimelineId = Guid.NewGuid(),
                RefundId = refund.RefundId,
                EventType = RefundTimelineEventType.REVERSED,
                ActedByUserId = command.ReversedByUserId,
                Comments = command.Request.Comments,
                EventDate = reversedDate
            };

            _db.RefundTimeline.Add(timelineEntry);
            await _db.SaveChangesAsync();

            // Publish RefundReversed event
            var refundEvent = new RefundReversed
            {
                RefundId = refund.RefundId,
                PaymentId = refund.PaymentId,
                QuotationId = refund.QuotationId,
                ReversedReason = command.Request.ReversedReason,
                ReversedByUserId = command.ReversedByUserId,
                ReversedDate = reversedDate
            };
            // TODO: Publish event via event bus

            _logger.LogInformation("Refund {RefundId} reversed by user {UserId}",
                refund.RefundId, command.ReversedByUserId);

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
                ReversedReason = refund.ReversedReason,
                ReversedDate = refund.ReversedDate,
                RequestDate = refund.RequestDate
            };
        }
    }
}

