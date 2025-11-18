using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.EventHandlers
{
    public class QuotationExpiringEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly ILogger<QuotationExpiringEventHandler> _logger;

        public QuotationExpiringEventHandler(
            IAppDbContext db,
            INotificationService notificationService,
            ILogger<QuotationExpiringEventHandler> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task HandleQuotationExpiring(Quotation quotation, int daysUntilExpiry)
        {
            try
            {
                if (quotation == null)
                    return;

                // Notify sales rep
                var meta = new Dictionary<string, object>
                {
                    { "QuotationNumber", quotation.QuotationNumber },
                    { "ClientName", quotation.Client?.CompanyName ?? "Unknown" },
                    { "ValidUntil", quotation.ValidUntil.ToString("MMM dd, yyyy") },
                    { "DaysUntilExpiry", daysUntilExpiry.ToString() }
                };

                await _notificationService.PublishNotificationAsync(
                    NotificationEventType.QuotationExpiring,
                    "Quotation",
                    quotation.QuotationId,
                    quotation.CreatedByUserId,
                    $"Quotation {quotation.QuotationNumber} is expiring in {daysUntilExpiry} day(s)",
                    meta,
                    new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                );

                // Also notify manager if exists
                if (quotation.CreatedByUser?.ReportingManagerId != null)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.QuotationExpiring,
                        "Quotation",
                        quotation.QuotationId,
                        quotation.CreatedByUser.ReportingManagerId.Value,
                        $"Quotation {quotation.QuotationNumber} is expiring in {daysUntilExpiry} day(s)",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                    );
                }

                _logger.LogInformation("Notification published for expiring quotation: {QuotationId}, Days: {Days}", 
                    quotation.QuotationId, daysUntilExpiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle expiring quotation notification for {QuotationId}", quotation?.QuotationId);
            }
        }
    }
}

