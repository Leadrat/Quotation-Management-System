using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using CRM.Domain.Enums;
using CRM.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.EventHandlers
{
    public class QuotationViewedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly ILogger<QuotationViewedEventHandler> _logger;

        public QuotationViewedEventHandler(
            IAppDbContext db,
            INotificationService notificationService,
            ILogger<QuotationViewedEventHandler> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(QuotationViewed evt)
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

                // Notify sales rep
                var meta = new Dictionary<string, object>
                {
                    { "QuotationNumber", quotation.QuotationNumber },
                    { "ClientName", quotation.Client?.CompanyName ?? "Unknown" }
                };

                await _notificationService.PublishNotificationAsync(
                    NotificationEventType.QuotationViewed,
                    "Quotation",
                    evt.QuotationId,
                    quotation.CreatedByUserId,
                    $"Client {quotation.Client?.CompanyName ?? "Unknown"} has viewed quotation {quotation.QuotationNumber}",
                    meta,
                    new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                );

                _logger.LogInformation("Notification published for QuotationViewed event: {QuotationId}", evt.QuotationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle QuotationViewed event for {QuotationId}", evt.QuotationId);
            }
        }
    }
}

