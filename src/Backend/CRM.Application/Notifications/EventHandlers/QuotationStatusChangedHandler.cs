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
    public class QuotationStatusChangedHandler
    {
        private readonly IAppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly ILogger<QuotationStatusChangedHandler> _logger;

        public QuotationStatusChangedHandler(
            IAppDbContext db,
            INotificationService notificationService,
            ILogger<QuotationStatusChangedHandler> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task HandleQuotationCreated(QuotationCreated evt)
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

                var meta = new Dictionary<string, object>
                {
                    { "QuotationNumber", evt.QuotationNumber },
                    { "ClientName", quotation.Client?.CompanyName ?? "Unknown" },
                    { "TotalAmount", evt.TotalAmount.ToString("C") },
                    { "CreatedAt", evt.CreatedAt.ToString("yyyy-MM-dd HH:mm") }
                };

                // Notify the sales rep who created the quotation
                await _notificationService.PublishNotificationAsync(
                    NotificationEventType.QuotationCreated,
                    "Quotation",
                    evt.QuotationId,
                    evt.CreatedByUserId,
                    $"New quotation {evt.QuotationNumber} created for {quotation.Client?.CompanyName ?? "client"}",
                    meta,
                    new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                );

                _logger.LogInformation("Notification published for QuotationCreated event: {QuotationId}", evt.QuotationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle QuotationCreated event for {QuotationId}", evt.QuotationId);
            }
        }

        public async Task HandleQuotationUpdated(QuotationUpdated evt)
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

                var meta = new Dictionary<string, object>
                {
                    { "QuotationNumber", quotation.QuotationNumber },
                    { "ClientName", quotation.Client?.CompanyName ?? "Unknown" },
                    { "UpdatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") }
                };

                // Notify the sales rep who created the quotation
                await _notificationService.PublishNotificationAsync(
                    NotificationEventType.QuotationUpdated,
                    "Quotation",
                    evt.QuotationId,
                    quotation.CreatedByUserId,
                    $"Quotation {quotation.QuotationNumber} has been updated",
                    meta,
                    new List<NotificationChannel> { NotificationChannel.InApp }
                );

                _logger.LogInformation("Notification published for QuotationUpdated event: {QuotationId}", evt.QuotationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle QuotationUpdated event for {QuotationId}", evt.QuotationId);
            }
        }

        public async Task HandleQuotationResponseReceived(QuotationResponseReceived evt)
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

                var meta = new Dictionary<string, object>
                {
                    { "QuotationNumber", quotation.QuotationNumber },
                    { "ClientName", quotation.Client?.CompanyName ?? "Unknown" },
                    { "ResponseType", evt.ResponseType },
                    { "ResponseDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") }
                };

                // Notify the sales rep who created the quotation
                await _notificationService.PublishNotificationAsync(
                    NotificationEventType.ClientResponse,
                    "Quotation",
                    evt.QuotationId,
                    quotation.CreatedByUserId,
                    $"Client response received for quotation {quotation.QuotationNumber}: {evt.ResponseType}",
                    meta,
                    new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                );

                _logger.LogInformation("Notification published for QuotationResponseReceived event: {QuotationId}", evt.QuotationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle QuotationResponseReceived event for {QuotationId}", evt.QuotationId);
            }
        }
    }
}