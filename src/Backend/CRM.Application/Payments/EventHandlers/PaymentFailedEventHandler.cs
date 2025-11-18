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
    public class PaymentFailedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentFailedEventHandler> _logger;

        public PaymentFailedEventHandler(
            IAppDbContext db,
            INotificationService notificationService,
            ILogger<PaymentFailedEventHandler> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(PaymentFailed evt)
        {
            try
            {
                var quotation = await _db.Quotations
                    .Include(q => q.CreatedByUser)
                    .FirstOrDefaultAsync(q => q.QuotationId == evt.QuotationId);

                if (quotation == null)
                {
                    _logger.LogWarning("Quotation {QuotationId} not found for payment failed notification", evt.QuotationId);
                    return;
                }

                // Notify sales rep
                if (quotation.CreatedByUserId != Guid.Empty)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.ClientResponse, // Using existing event type
                        "Quotation",
                        evt.QuotationId,
                        quotation.CreatedByUserId,
                        $"Payment failed for Quotation #{quotation.QuotationNumber}. Reason: {evt.FailureReason}",
                        new Dictionary<string, object>
                        {
                            { "PaymentId", evt.PaymentId },
                            { "FailureReason", evt.FailureReason ?? "" },
                            { "Event", "PaymentFailed" }
                        });
                }

                _logger.LogInformation("Payment failed notification sent for payment {PaymentId}", evt.PaymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment failed event for payment {PaymentId}", evt.PaymentId);
            }
        }
    }
}

