using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.EventHandlers
{
    public class QuotationSentEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly ILogger<QuotationSentEventHandler> _logger;

        public QuotationSentEventHandler(
            IAppDbContext db,
            INotificationService notificationService,
            ILogger<QuotationSentEventHandler> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(QuotationSent evt)
        {
            try
            {
                var quotation = await _db.Quotations
                    .Include(q => q.Client)
                    .Include(q => q.CreatedByUser)
                    .FirstOrDefaultAsync(q => q.QuotationId == evt.QuotationId);

                if (quotation == null)
                {
                    _logger.LogWarning("Quotation {QuotationId} not found for notification", evt.QuotationId);
                    return;
                }

                // Notify sales rep who created the quotation
                var meta = new Dictionary<string, object>
                {
                    { "QuotationNumber", quotation.QuotationNumber },
                    { "ClientName", quotation.Client?.CompanyName ?? "Unknown" },
                    { "TotalAmount", quotation.TotalAmount.ToString("C") }
                };

                await _notificationService.PublishNotificationAsync(
                    NotificationEventType.QuotationSent,
                    "Quotation",
                    evt.QuotationId,
                    quotation.CreatedByUserId,
                    $"Quotation {quotation.QuotationNumber} has been sent to {quotation.Client?.CompanyName ?? evt.RecipientEmail}",
                    meta,
                    new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                );

                _logger.LogInformation("Notification published for QuotationSent event: {QuotationId}", evt.QuotationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle QuotationSent event for {QuotationId}", evt.QuotationId);
                // Don't throw - notification failure shouldn't break the main workflow
            }
        }
    }
}

