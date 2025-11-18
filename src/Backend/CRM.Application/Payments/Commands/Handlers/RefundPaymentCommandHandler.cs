using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Dtos;
using CRM.Application.Payments.Services;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Payments.Commands.Handlers
{
    public class RefundPaymentCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly ILogger<RefundPaymentCommandHandler> _logger;

        public RefundPaymentCommandHandler(
            IAppDbContext db,
            IPaymentGatewayFactory gatewayFactory,
            ILogger<RefundPaymentCommandHandler> logger)
        {
            _db = db;
            _gatewayFactory = gatewayFactory;
            _logger = logger;
        }

        public async Task<PaymentDto> Handle(RefundPaymentCommand command)
        {
            var payment = await _db.Payments
                .Include(p => p.Quotation)
                .FirstOrDefaultAsync(p => p.PaymentId == command.PaymentId);

            if (payment == null)
                throw new InvalidOperationException("Payment not found");

            if (!payment.CanBeRefunded())
                throw new InvalidOperationException("Payment cannot be refunded");

            // Determine refund amount (full or partial)
            var refundAmount = command.Request.Amount ?? (payment.AmountPaid - (payment.RefundAmount ?? 0));

            if (refundAmount <= 0)
                throw new InvalidOperationException("Refund amount must be greater than zero");

            if (refundAmount > payment.AmountPaid - (payment.RefundAmount ?? 0))
                throw new InvalidOperationException("Refund amount exceeds available refundable amount");

            // Get gateway service
            var gatewayService = await _gatewayFactory.GetGatewayServiceAsync(payment.PaymentGateway);
            if (gatewayService == null)
                throw new InvalidOperationException($"Payment gateway '{payment.PaymentGateway}' not found");

            // Get gateway config
            var gatewayConfig = await _db.PaymentGatewayConfigs
                .FirstOrDefaultAsync(c => c.GatewayName == payment.PaymentGateway && c.Enabled);

            if (gatewayConfig == null)
                throw new InvalidOperationException($"Payment gateway '{payment.PaymentGateway}' is not configured");

            try
            {
                // Call gateway refund service
                var refundResponse = await gatewayService.RefundPaymentAsync(
                    payment.PaymentReference,
                    refundAmount,
                    command.Request.Reason,
                    gatewayConfig.ApiKey,
                    gatewayConfig.ApiSecret,
                    gatewayConfig.IsTestMode);

                if (refundResponse.Success)
                {
                    // Update payment entity
                    payment.ProcessRefund(refundAmount, command.Request.Reason);

                    await _db.SaveChangesAsync();

                    // Publish PaymentRefunded event
                    var refundEvent = new PaymentRefunded
                    {
                        PaymentId = payment.PaymentId,
                        QuotationId = payment.QuotationId,
                        PaymentGateway = payment.PaymentGateway,
                        RefundAmount = refundAmount,
                        TotalAmountPaid = payment.AmountPaid,
                        IsPartialRefund = payment.PaymentStatus == Domain.Enums.PaymentStatus.PartiallyRefunded,
                        RefundReason = command.Request.Reason,
                        RefundDate = refundResponse.RefundedAt,
                        RefundedByUserId = command.RefundedByUserId
                    };
                    // TODO: Publish event via event bus

                    _logger.LogInformation("Payment {PaymentId} refunded: {RefundAmount} of {TotalAmount}",
                        payment.PaymentId, refundAmount, payment.AmountPaid);

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
                        IsRefundable = payment.IsRefundable,
                        RefundAmount = payment.RefundAmount,
                        RefundReason = payment.RefundReason,
                        RefundDate = payment.RefundDate,
                        CanBeRefunded = payment.CanBeRefunded(),
                        CanBeCancelled = payment.CanBeCancelled()
                    };
                }
                else
                {
                    throw new InvalidOperationException(refundResponse.ErrorMessage ?? "Refund failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", payment.PaymentId);
                throw;
            }
        }
    }
}

