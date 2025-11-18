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
    public class QuotationResponseReceivedEventHandler
    {
        private readonly IAppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly ILogger<QuotationResponseReceivedEventHandler> _logger;

        public QuotationResponseReceivedEventHandler(
            IAppDbContext db,
            INotificationService notificationService,
            ILogger<QuotationResponseReceivedEventHandler> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(QuotationResponseReceived evt)
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

                var response = await _db.QuotationResponses
                    .FirstOrDefaultAsync(r => r.ResponseId == evt.ResponseId);

                if (response == null)
                {
                    _logger.LogWarning("Response {ResponseId} not found for notification", evt.ResponseId);
                    return;
                }

                var isAccepted = response.ResponseType == "ACCEPTED";
                var eventType = isAccepted ? NotificationEventType.QuotationAccepted : NotificationEventType.QuotationRejected;

                // Notify sales rep
                var meta = new Dictionary<string, object>
                {
                    { "QuotationNumber", quotation.QuotationNumber },
                    { "ClientName", quotation.Client?.CompanyName ?? response.ClientName ?? "Unknown" },
                    { "TotalAmount", quotation.TotalAmount.ToString("C") },
                    { "ResponseType", response.ResponseType }
                };

                var message = isAccepted
                    ? $"Great news! {quotation.Client?.CompanyName ?? response.ClientName ?? "Client"} has accepted quotation {quotation.QuotationNumber}"
                    : $"{quotation.Client?.CompanyName ?? response.ClientName ?? "Client"} has rejected quotation {quotation.QuotationNumber}";

                await _notificationService.PublishNotificationAsync(
                    eventType,
                    "Quotation",
                    evt.QuotationId,
                    quotation.CreatedByUserId,
                    message,
                    meta,
                    new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                );

                // Also notify manager if exists
                if (quotation.CreatedByUser?.ReportingManagerId != null)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.ClientResponse,
                        "Quotation",
                        evt.QuotationId,
                        quotation.CreatedByUser.ReportingManagerId.Value,
                        $"Client response received for quotation {quotation.QuotationNumber}",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                    );
                }

                _logger.LogInformation("Notification published for QuotationResponseReceived event: {QuotationId}, Type: {ResponseType}",
                    evt.QuotationId, response.ResponseType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle QuotationResponseReceived event for {QuotationId}", evt.QuotationId);
            }
        }
    }
}

