using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.EventHandlers;
using CRM.Application.Notifications.Services;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Jobs
{
    public class QuotationExpirationNotificationJob : CronBackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QuotationExpirationNotificationJob> _logger;

        public QuotationExpirationNotificationJob(
            IServiceProvider serviceProvider,
            ILogger<QuotationExpirationNotificationJob> logger)
            : base("0 0 9 * * ?", null, logger) // Daily at 9 AM
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ProcessAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QuotationExpirationNotificationJob started at {Time}", DateTimeOffset.UtcNow);

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<CRM.Application.Notifications.Services.INotificationService>();
            var expiringHandler = scope.ServiceProvider.GetRequiredService<QuotationExpiringEventHandler>();

            var today = DateTime.Today;
            var twoDaysFromNow = today.AddDays(2);
            var oneDayFromNow = today.AddDays(1);

            // Find quotations expiring in next 24-48 hours
            var expiringQuotations = await db.Quotations
                .Include(q => q.Client)
                .Include(q => q.CreatedByUser)
                .Where(q => q.Status == QuotationStatus.Sent &&
                           q.ValidUntil >= oneDayFromNow &&
                           q.ValidUntil <= twoDaysFromNow)
                .ToListAsync(stoppingToken);

            foreach (var quotation in expiringQuotations)
            {
                try
                {
                    var daysUntilExpiry = (quotation.ValidUntil - today).Days;
                    await expiringHandler.HandleQuotationExpiring(quotation, daysUntilExpiry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send expiration notification for quotation {QuotationId}", quotation.QuotationId);
                }
            }

            // Also check for expired quotations (expired yesterday or today)
            var expiredQuotations = await db.Quotations
                .Include(q => q.Client)
                .Include(q => q.CreatedByUser)
                .Where(q => q.Status != QuotationStatus.Expired &&
                           q.ValidUntil < today)
                .ToListAsync(stoppingToken);

            foreach (var quotation in expiredQuotations)
            {
                try
                {
                    var meta = new Dictionary<string, object>
                    {
                        { "QuotationNumber", quotation.QuotationNumber },
                        { "ClientName", quotation.Client?.CompanyName ?? "Unknown" },
                        { "ValidUntil", quotation.ValidUntil.ToString("MMM dd, yyyy") }
                    };

                    await notificationService.PublishNotificationAsync(
                        NotificationEventType.QuotationExpired,
                        "Quotation",
                        quotation.QuotationId,
                        quotation.CreatedByUserId,
                        $"Quotation {quotation.QuotationNumber} has expired",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send expired notification for quotation {QuotationId}", quotation.QuotationId);
                }
            }

            _logger.LogInformation("QuotationExpirationNotificationJob finished. Processed {ExpiringCount} expiring and {ExpiredCount} expired quotations",
                expiringQuotations.Count, expiredQuotations.Count);
        }
    }
}

