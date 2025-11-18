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
    public class PaymentSuccessEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentSuccessEventHandler> _logger;

        public PaymentSuccessEventHandler(
            IAppDbContext db,
            INotificationService notificationService,
            ILogger<PaymentSuccessEventHandler> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(PaymentSuccess evt)
        {
            try
            {
                // Get quotation and related users
                var quotation = await _db.Quotations
                    .Include(q => q.Client)
                    .Include(q => q.CreatedByUser)
                    .FirstOrDefaultAsync(q => q.QuotationId == evt.QuotationId);

                if (quotation == null)
                {
                    _logger.LogWarning("Quotation {QuotationId} not found for payment success notification", evt.QuotationId);
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
                        $"Payment of {evt.Currency} {evt.AmountPaid:N2} received for Quotation #{quotation.QuotationNumber}",
                        new Dictionary<string, object>
                        {
                            { "PaymentId", evt.PaymentId },
                            { "Amount", evt.AmountPaid },
                            { "Currency", evt.Currency },
                            { "Event", "PaymentSuccess" }
                        });
                }

                _logger.LogInformation("Payment success notification sent for payment {PaymentId}", evt.PaymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment success event for payment {PaymentId}", evt.PaymentId);
            }
        }
    }
}

