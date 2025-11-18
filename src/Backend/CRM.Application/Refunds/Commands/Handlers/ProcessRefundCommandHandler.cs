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
    public class ProcessRefundCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly ILogger<ProcessRefundCommandHandler> _logger;

        public ProcessRefundCommandHandler(
            IAppDbContext db,
            IPaymentGatewayFactory gatewayFactory,
            ILogger<ProcessRefundCommandHandler> logger)
        {
            _db = db;
            _gatewayFactory = gatewayFactory;
            _logger = logger;
        }

        public async Task<RefundDto> Handle(ProcessRefundCommand command)
        {
            var refund = await _db.Refunds
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.RefundId == command.RefundId);

            if (refund == null)
                throw new InvalidOperationException("Refund not found");

            if (refund.RefundStatus != RefundStatus.Approved)
                throw new InvalidOperationException("Refund must be approved before processing");

            // Get gateway service
            var gatewayService = await _gatewayFactory.GetGatewayServiceAsync(refund.Payment.PaymentGateway);
            if (gatewayService == null)
                throw new InvalidOperationException($"Payment gateway '{refund.Payment.PaymentGateway}' not found");

            // Get gateway config
            var gatewayConfig = await _db.PaymentGatewayConfigs
                .FirstOrDefaultAsync(c => c.GatewayName == refund.Payment.PaymentGateway && c.Enabled);

            if (gatewayConfig == null)
                throw new InvalidOperationException($"Payment gateway '{refund.Payment.PaymentGateway}' is not configured");

            refund.MarkAsProcessing();

            // Create timeline entry
            var processingTimeline = new RefundTimeline
            {
                TimelineId = Guid.NewGuid(),
                RefundId = refund.RefundId,
                EventType = RefundTimelineEventType.PROCESSING,
                ActedByUserId = refund.RequestedByUserId,
                EventDate = DateTimeOffset.UtcNow
            };

            _db.RefundTimeline.Add(processingTimeline);
            await _db.SaveChangesAsync();

            try
            {
                // Call gateway refund API
                var refundResponse = await gatewayService.RefundPaymentAsync(
                    refund.Payment.PaymentReference,
                    refund.RefundAmount,
                    refund.RefundReason,
                    gatewayConfig.ApiKey,
                    gatewayConfig.ApiSecret,
                    gatewayConfig.IsTestMode);

                if (refundResponse.Success)
                {
                    var completedDate = DateTimeOffset.UtcNow;
                    refund.MarkAsCompleted(refundResponse.RefundReference, completedDate);

                    // Update payment entity
                    refund.Payment.ProcessRefund(refund.RefundAmount, refund.RefundReason);

                    // Create timeline entry
                    var completedTimeline = new RefundTimeline
                    {
                        TimelineId = Guid.NewGuid(),
                        RefundId = refund.RefundId,
                        EventType = RefundTimelineEventType.COMPLETED,
                        ActedByUserId = refund.RequestedByUserId,
                        EventDate = completedDate
                    };

                    _db.RefundTimeline.Add(completedTimeline);
                    await _db.SaveChangesAsync();

                    // Publish RefundCompleted event
                    var refundEvent = new RefundCompleted
                    {
                        RefundId = refund.RefundId,
                        PaymentId = refund.PaymentId,
                        QuotationId = refund.QuotationId,
                        RefundAmount = refund.RefundAmount,
                        PaymentGatewayReference = refundResponse.RefundReference,
                        CompletedDate = completedDate
                    };
                    // TODO: Publish event via event bus

                    _logger.LogInformation("Refund {RefundId} processed successfully",
                        refund.RefundId);

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
                        PaymentGatewayReference = refund.PaymentGatewayReference,
                        RequestDate = refund.RequestDate,
                        CompletedDate = refund.CompletedDate
                    };
                }
                else
                {
                    refund.MarkAsFailed(refundResponse.ErrorMessage ?? "Refund processing failed");
                    
                    var failedTimeline = new RefundTimeline
                    {
                        TimelineId = Guid.NewGuid(),
                        RefundId = refund.RefundId,
                        EventType = RefundTimelineEventType.FAILED,
                        ActedByUserId = refund.RequestedByUserId,
                        Comments = refundResponse.ErrorMessage,
                        EventDate = DateTimeOffset.UtcNow
                    };

                    _db.RefundTimeline.Add(failedTimeline);
                    await _db.SaveChangesAsync();

                    // Publish RefundFailed event
                    var refundEvent = new RefundFailed
                    {
                        RefundId = refund.RefundId,
                        PaymentId = refund.PaymentId,
                        QuotationId = refund.QuotationId,
                        FailureReason = refundResponse.ErrorMessage ?? "Refund processing failed",
                        FailedDate = DateTimeOffset.UtcNow
                    };
                    // TODO: Publish event via event bus

                    throw new InvalidOperationException(refundResponse.ErrorMessage ?? "Refund processing failed");
                }
            }
            catch (Exception ex)
            {
                refund.MarkAsFailed(ex.Message);
                await _db.SaveChangesAsync();

                _logger.LogError(ex, "Error processing refund {RefundId}", refund.RefundId);
                throw;
            }
        }
    }
}

