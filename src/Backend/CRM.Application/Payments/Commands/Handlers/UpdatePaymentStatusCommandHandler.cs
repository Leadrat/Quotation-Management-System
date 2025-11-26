using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Dtos;
using CRM.Application.Payments.Services;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Payments.Commands.Handlers
{
    public class UpdatePaymentStatusCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly ILogger<UpdatePaymentStatusCommandHandler> _logger;
        private readonly ITenantContext _tenantContext;

        public UpdatePaymentStatusCommandHandler(
            IAppDbContext db,
            IPaymentGatewayFactory gatewayFactory,
            ILogger<UpdatePaymentStatusCommandHandler> logger,
            ITenantContext tenantContext)
        {
            _db = db;
            _gatewayFactory = gatewayFactory;
            _logger = logger;
            _tenantContext = tenantContext;
        }

        public async Task<PaymentDto> Handle(UpdatePaymentStatusCommand command)
        {
            var request = command.Request;
            var currentTenantId = _tenantContext.CurrentTenantId;

            // Find payment by reference
            // Temporarily disable tenant filter for debugging
            // var currentTenantId = _tenantContext.CurrentTenantId;
            var payment = await _db.Payments
                .Include(p => p.Quotation)
                // .FirstOrDefaultAsync(p => p.PaymentReference == request.PaymentReference && p.TenantId == currentTenantId)
                .FirstOrDefaultAsync(p => p.PaymentReference == request.PaymentReference);

            if (payment == null)
                throw new InvalidOperationException($"Payment with reference '{request.PaymentReference}' not found");

            // Verify payment status with gateway (optional but recommended)
            if (!string.IsNullOrEmpty(command.GatewayName))
            {
                var gatewayService = await _gatewayFactory.GetGatewayServiceAsync(command.GatewayName);
                if (gatewayService != null)
                {
                    try
                    {
                        var gatewayConfig = await _db.PaymentGatewayConfigs
                            .FirstOrDefaultAsync(c => c.GatewayName == command.GatewayName && c.Enabled);

                        if (gatewayConfig != null)
                        {
                            var verification = await gatewayService.VerifyPaymentAsync(
                                request.PaymentReference,
                                gatewayConfig.ApiKey,
                                gatewayConfig.ApiSecret,
                                gatewayConfig.IsTestMode);

                            // Use verified status if available
                            if (verification.IsValid)
                            {
                                request.Status = verification.Status;
                                if (verification.PaymentDate.HasValue)
                                    request.PaymentDate = verification.PaymentDate;
                                if (!string.IsNullOrEmpty(verification.FailureReason))
                                    request.FailureReason = verification.FailureReason;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to verify payment with gateway, using provided status");
                    }
                }
            }

            // Update payment status based on request
            var previousStatus = payment.PaymentStatus;

            switch (request.Status.ToLowerInvariant())
            {
                case "success":
                case "succeeded":
                case "completed":
                    if (payment.PaymentStatus != PaymentStatus.Success)
                    {
                        payment.MarkAsSuccess(request.PaymentDate ?? DateTimeOffset.UtcNow);
                        
                        // Update quotation status if needed
                        if (payment.Quotation.Status != Domain.Enums.QuotationStatus.Accepted)
                        {
                            // Quotation should already be accepted, but update if needed
                            _logger.LogInformation("Payment succeeded for quotation {QuotationId}", payment.QuotationId);
                        }

                        // Publish PaymentSuccess event
                        var successEvent = new PaymentSuccess
                        {
                            PaymentId = payment.PaymentId,
                            QuotationId = payment.QuotationId,
                            PaymentGateway = payment.PaymentGateway,
                            PaymentReference = payment.PaymentReference,
                            AmountPaid = payment.AmountPaid,
                            Currency = payment.Currency,
                            PaymentDate = payment.PaymentDate ?? DateTimeOffset.UtcNow
                        };
                        // TODO: Publish event via event bus
                    }
                    break;

                case "failed":
                case "failure":
                    if (payment.PaymentStatus != PaymentStatus.Failed)
                    {
                        payment.MarkAsFailed(request.FailureReason ?? "Payment failed");
                        
                        // Publish PaymentFailed event
                        var failedEvent = new PaymentFailed
                        {
                            PaymentId = payment.PaymentId,
                            QuotationId = payment.QuotationId,
                            PaymentGateway = payment.PaymentGateway,
                            PaymentReference = payment.PaymentReference,
                            AmountPaid = payment.AmountPaid,
                            Currency = payment.Currency,
                            FailureReason = payment.FailureReason ?? "Payment failed",
                            FailedAt = DateTimeOffset.UtcNow
                        };
                        // TODO: Publish event via event bus
                    }
                    break;

                case "pending":
                    payment.MarkAsProcessing();
                    break;

                default:
                    _logger.LogWarning("Unknown payment status: {Status}", request.Status);
                    break;
            }

            // Update metadata if provided
            if (request.Metadata != null && request.Metadata.Any())
            {
                payment.Metadata = System.Text.Json.JsonSerializer.Serialize(request.Metadata);
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Payment {PaymentId} status updated from {PreviousStatus} to {NewStatus}",
                payment.PaymentId, previousStatus, payment.PaymentStatus);

            return new PaymentDto
            {
                PaymentId = payment.PaymentId,
                QuotationId = payment.QuotationId,
                PaymentGateway = payment.PaymentGateway,
                PaymentReference = payment.PaymentReference,
                AmountPaid = payment.AmountPaid,
                Currency = payment.Currency,
                PaymentStatus = payment.PaymentStatus,
                PaymentDate = payment.PaymentDate,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt,
                FailureReason = payment.FailureReason,
                IsRefundable = payment.IsRefundable,
                RefundAmount = payment.RefundAmount,
                RefundReason = payment.RefundReason,
                RefundDate = payment.RefundDate,
                CanBeRefunded = payment.CanBeRefunded(),
                CanBeCancelled = payment.CanBeCancelled()
            };
        }
    }
}

