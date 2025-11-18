using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Payments.EventHandlers
{
    public class PaymentRefundedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentRefundedEventHandler> _logger;

        public PaymentRefundedEventHandler(
            IAppDbContext db,
            INotificationService notificationService,
            ILogger<PaymentRefundedEventHandler> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(PaymentRefunded evt)
        {
            try
            {
                var quotation = await _db.Quotations
                    .Include(q => q.CreatedByUser)
                    .FirstOrDefaultAsync(q => q.QuotationId == evt.QuotationId);

                if (quotation == null)
                {
                    _logger.LogWarning("Quotation {QuotationId} not found for payment refunded notification", evt.QuotationId);
                    return;
                }

                // Notify sales rep
                if (quotation.CreatedByUserId != Guid.Empty)
                {
                    var message = evt.IsPartialRefund
                        ? $"Partial refund of {evt.RefundAmount:N2} processed for Quotation #{quotation.QuotationNumber}"
                        : $"Full refund of {evt.RefundAmount:N2} processed for Quotation #{quotation.QuotationNumber}";

                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.ClientResponse, // Using existing event type
                        "Quotation",
                        evt.QuotationId,
                        quotation.CreatedByUserId,
                        message,
                        new Dictionary<string, object>
                        {
                            { "PaymentId", evt.PaymentId },
                            { "RefundAmount", evt.RefundAmount },
                            { "IsPartial", evt.IsPartialRefund },
                            { "Event", "PaymentRefunded" }
                        });
                }

                _logger.LogInformation("Payment refunded notification sent for payment {PaymentId}", evt.PaymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment refunded event for payment {PaymentId}", evt.PaymentId);
            }
        }
    }
}

