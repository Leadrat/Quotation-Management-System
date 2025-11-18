using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Payments.Dtos;
using CRM.Application.Payments.Services;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Payments.Commands.Handlers
{
    public class CancelPaymentCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly ILogger<CancelPaymentCommandHandler> _logger;

        public CancelPaymentCommandHandler(
            IAppDbContext db,
            IPaymentGatewayFactory gatewayFactory,
            ILogger<CancelPaymentCommandHandler> logger)
        {
            _db = db;
            _gatewayFactory = gatewayFactory;
            _logger = logger;
        }

        public async Task<PaymentDto> Handle(CancelPaymentCommand command)
        {
            var payment = await _db.Payments
                .Include(p => p.Quotation)
                .FirstOrDefaultAsync(p => p.PaymentId == command.PaymentId);

            if (payment == null)
                throw new InvalidOperationException("Payment not found");

            if (!payment.CanBeCancelled())
                throw new InvalidOperationException("Payment cannot be cancelled");

            // Get gateway service (may not be needed for all gateways)
            var gatewayService = await _gatewayFactory.GetGatewayServiceAsync(payment.PaymentGateway);
            
            if (gatewayService != null)
            {
                try
                {
                    var gatewayConfig = await _db.PaymentGatewayConfigs
                        .FirstOrDefaultAsync(c => c.GatewayName == payment.PaymentGateway && c.Enabled);

                    if (gatewayConfig != null)
                    {
                        // Attempt to cancel with gateway (if supported)
                        await gatewayService.CancelPaymentAsync(
                            payment.PaymentReference,
                            gatewayConfig.ApiKey,
                            gatewayConfig.ApiSecret,
                            gatewayConfig.IsTestMode);
                    }
                }
                catch (NotImplementedException)
                {
                    // Gateway doesn't support cancellation, continue with local cancellation
                    _logger.LogInformation("Gateway {Gateway} does not support cancellation, cancelling locally",
                        payment.PaymentGateway);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cancel payment with gateway, cancelling locally");
                }
            }

            // Cancel payment locally
            payment.Cancel();
            await _db.SaveChangesAsync();

            // Publish PaymentCancelled event
            var cancelledEvent = new PaymentCancelled
            {
                PaymentId = payment.PaymentId,
                QuotationId = payment.QuotationId,
                PaymentGateway = payment.PaymentGateway,
                AmountPaid = payment.AmountPaid,
                CancelledAt = DateTimeOffset.UtcNow,
                CancelledByUserId = command.CancelledByUserId
            };
            // TODO: Publish event via event bus

            _logger.LogInformation("Payment {PaymentId} cancelled", payment.PaymentId);

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
                CanBeRefunded = payment.CanBeRefunded(),
                CanBeCancelled = payment.CanBeCancelled()
            };
        }
    }
}

