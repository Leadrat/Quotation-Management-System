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
    public class PaymentRequestHandler
    {
        private readonly IAppDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentRequestHandler> _logger;

        public PaymentRequestHandler(
            IAppDbContext db,
            INotificationService notificationService,
            ILogger<PaymentRequestHandler> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task HandlePaymentRequested(PaymentRequested evt)
        {
            try
            {
                var quotation = await _db.Quotations
                    .Include(q => q.Client)
                    .Include(q => q.CreatedByUser)
                    .FirstOrDefaultAsync(q => q.QuotationId == evt.QuotationId);

                if (quotation == null)
                {
                    _logger.LogWarning("Quotation {QuotationId} not found for payment notification", evt.QuotationId);
                    return;
                }

                var meta = new Dictionary<string, object>
                {
                    { "QuotationNumber", quotation.QuotationNumber },
                    { "ClientName", quotation.Client?.CompanyName ?? "Unknown" },
                    { "PaymentAmount", evt.Amount.ToString("C") },
                    { "DueDate", evt.DueDate.ToString("yyyy-MM-dd") },
                    { "PaymentMethod", evt.PaymentMethod ?? "Not specified" },
                    { "RequestedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") }
                };

                // Notify the client (if they have a user account)
                if (quotation.Client?.CreatedByUserId != null)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.PaymentRequested,
                        "Payment",
                        evt.PaymentId,
                        quotation.Client.CreatedByUserId,
                        $"Payment requested for quotation {quotation.QuotationNumber} - Amount: {evt.Amount:C}",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                    );
                }

                // Notify the sales rep
                await _notificationService.PublishNotificationAsync(
                    NotificationEventType.PaymentRequested,
                    "Payment",
                    evt.PaymentId,
                    quotation.CreatedByUserId,
                    $"Payment request sent to {quotation.Client?.CompanyName} for quotation {quotation.QuotationNumber}",
                    meta,
                    new List<NotificationChannel> { NotificationChannel.InApp }
                );

                _logger.LogInformation("Notification published for PaymentRequested event: {PaymentId}", evt.PaymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle PaymentRequested event for {PaymentId}", evt.PaymentId);
            }
        }

        public async Task HandlePaymentReceived(PaymentReceived evt)
        {
            try
            {
                var payment = await _db.Payments
                    .Include(p => p.Quotation)
                        .ThenInclude(q => q.Client)
                    .Include(p => p.Quotation.CreatedByUser)
                    .FirstOrDefaultAsync(p => p.PaymentId == evt.PaymentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found for notification", evt.PaymentId);
                    return;
                }

                var meta = new Dictionary<string, object>
                {
                    { "PaymentId", evt.PaymentId.ToString() },
                    { "QuotationNumber", payment.Quotation?.QuotationNumber ?? "Unknown" },
                    { "ClientName", payment.Quotation?.Client?.CompanyName ?? "Unknown" },
                    { "PaymentAmount", evt.Amount.ToString("C") },
                    { "PaymentMethod", evt.PaymentMethod ?? "Not specified" },
                    { "TransactionId", evt.TransactionId ?? "N/A" },
                    { "ReceivedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") }
                };

                // Notify the sales rep
                if (payment.Quotation?.CreatedByUserId != null)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.PaymentReceived,
                        "Payment",
                        evt.PaymentId,
                        payment.Quotation.CreatedByUserId,
                        $"Payment received from {payment.Quotation.Client?.CompanyName} - Amount: {evt.Amount:C}",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                    );
                }

                // Notify finance team (assuming there's a finance role)
                var financeUsers = await _db.Users
                    .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Finance"))
                    .ToListAsync();

                foreach (var financeUser in financeUsers)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.PaymentReceived,
                        "Payment",
                        evt.PaymentId,
                        financeUser.UserId,
                        $"Payment received: {evt.Amount:C} for quotation {payment.Quotation?.QuotationNumber}",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp }
                    );
                }

                _logger.LogInformation("Notification published for PaymentReceived event: {PaymentId}", evt.PaymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle PaymentReceived event for {PaymentId}", evt.PaymentId);
            }
        }

        public async Task HandlePaymentFailed(PaymentFailed evt)
        {
            try
            {
                var payment = await _db.Payments
                    .Include(p => p.Quotation)
                        .ThenInclude(q => q.Client)
                    .Include(p => p.Quotation.CreatedByUser)
                    .FirstOrDefaultAsync(p => p.PaymentId == evt.PaymentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found for notification", evt.PaymentId);
                    return;
                }

                var meta = new Dictionary<string, object>
                {
                    { "PaymentId", evt.PaymentId.ToString() },
                    { "QuotationNumber", payment.Quotation?.QuotationNumber ?? "Unknown" },
                    { "ClientName", payment.Quotation?.Client?.CompanyName ?? "Unknown" },
                    { "PaymentAmount", payment.AmountPaid.ToString("C") },
                    { "FailureReason", evt.FailureReason ?? "Unknown error" },
                    { "FailedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") }
                };

                // Notify the client (if they have a user account)
                if (payment.Quotation?.Client?.CreatedByUserId != null)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.PaymentFailed,
                        "Payment",
                        evt.PaymentId,
                        payment.Quotation.Client.CreatedByUserId,
                        $"Payment failed for quotation {payment.Quotation.QuotationNumber}. Please try again.",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                    );
                }

                // Notify the sales rep
                if (payment.Quotation?.CreatedByUserId != null)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.PaymentFailed,
                        "Payment",
                        evt.PaymentId,
                        payment.Quotation.CreatedByUserId,
                        $"Payment failed for {payment.Quotation.Client?.CompanyName} - Quotation: {payment.Quotation.QuotationNumber}",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                    );
                }

                _logger.LogInformation("Notification published for PaymentFailed event: {PaymentId}", evt.PaymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle PaymentFailed event for {PaymentId}", evt.PaymentId);
            }
        }

        public async Task HandlePaymentOverdue(PaymentOverdue evt)
        {
            try
            {
                var payment = await _db.Payments
                    .Include(p => p.Quotation)
                        .ThenInclude(q => q.Client)
                    .Include(p => p.Quotation.CreatedByUser)
                    .FirstOrDefaultAsync(p => p.PaymentId == evt.PaymentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found for overdue notification", evt.PaymentId);
                    return;
                }

                var daysPastDue = (DateTime.UtcNow.Date - evt.DueDate.Date).Days;

                var meta = new Dictionary<string, object>
                {
                    { "PaymentId", evt.PaymentId.ToString() },
                    { "QuotationNumber", payment.Quotation?.QuotationNumber ?? "Unknown" },
                    { "ClientName", payment.Quotation?.Client?.CompanyName ?? "Unknown" },
                    { "PaymentAmount", payment.AmountPaid.ToString("C") },
                    { "DueDate", evt.DueDate.ToString("yyyy-MM-dd") },
                    { "DaysPastDue", daysPastDue.ToString() },
                    { "OverdueAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") }
                };

                // Notify the client (if they have a user account)
                if (payment.Quotation?.Client?.CreatedByUserId != null)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.PaymentOverdue,
                        "Payment",
                        evt.PaymentId,
                        payment.Quotation.Client.CreatedByUserId,
                        $"Payment overdue: {payment.AmountPaid:C} for quotation {payment.Quotation.QuotationNumber} ({daysPastDue} days past due)",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.SMS }
                    );
                }

                // Notify the sales rep
                if (payment.Quotation?.CreatedByUserId != null)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.PaymentOverdue,
                        "Payment",
                        evt.PaymentId,
                        payment.Quotation.CreatedByUserId,
                        $"Payment overdue from {payment.Quotation.Client?.CompanyName} - {daysPastDue} days past due",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
                    );
                }

                // Notify finance team for overdue payments
                var financeUsers = await _db.Users
                    .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Finance"))
                    .ToListAsync();

                foreach (var financeUser in financeUsers)
                {
                    await _notificationService.PublishNotificationAsync(
                        NotificationEventType.PaymentOverdue,
                        "Payment",
                        evt.PaymentId,
                        financeUser.UserId,
                        $"Overdue payment: {payment.AmountPaid:C} from {payment.Quotation?.Client?.CompanyName} ({daysPastDue} days)",
                        meta,
                        new List<NotificationChannel> { NotificationChannel.InApp }
                    );
                }

                _logger.LogInformation("Notification published for PaymentOverdue event: {PaymentId}", evt.PaymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle PaymentOverdue event for {PaymentId}", evt.PaymentId);
            }
        }
    }
}