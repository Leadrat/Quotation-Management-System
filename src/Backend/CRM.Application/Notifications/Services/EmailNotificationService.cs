using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IAppDbContext _db;
        private readonly IFluentEmail _fluentEmail;
        private readonly INotificationTemplateService _templateService;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(
            IAppDbContext db,
            IFluentEmail fluentEmail,
            INotificationTemplateService templateService,
            ILogger<EmailNotificationService> logger)
        {
            _db = db;
            _fluentEmail = fluentEmail;
            _templateService = templateService;
            _logger = logger;
        }

        public async Task SendEmailNotificationAsync(Notification notification, User recipientUser)
        {
            try
            {
                // Get template for event type
                var eventType = Enum.Parse<NotificationEventType>(notification.EventType);
                var template = _templateService.GetTemplate(eventType);

                // Replace placeholders (simplified - in production, load entity data)
                var subject = template.GetSubject(notification);
                var body = template.GetBody(notification);

                // Send email
                var emailResult = await _fluentEmail
                    .To(recipientUser.Email)
                    .Subject(subject)
                    .Body(body, true)
                    .SendAsync();

                // Log email
                var log = new EmailNotificationLog
                {
                    LogId = Guid.NewGuid(),
                    NotificationId = notification.NotificationId,
                    RecipientEmail = recipientUser.Email,
                    EventType = notification.EventType,
                    Subject = subject,
                    SentAt = DateTimeOffset.UtcNow,
                    Status = emailResult.Successful ? "SENT" : "FAILED",
                    ErrorMsg = emailResult.Successful ? null : string.Join(", ", emailResult.ErrorMessages),
                    RetryCount = 0
                };

                if (emailResult.Successful)
                {
                    log.MarkAsDelivered();
                }
                else
                {
                    log.MarkAsFailed(string.Join(", ", emailResult.ErrorMessages));
                }

                _db.EmailNotificationLogs.Add(log);
                await _db.SaveChangesAsync();

                // Update notification delivery status
                if (emailResult.Successful)
                {
                    notification.DeliveryStatus = "DELIVERED";
                }
                else
                {
                    notification.DeliveryStatus = "FAILED";
                }
                await _db.SaveChangesAsync();

                _logger.LogInformation("Email notification sent for {NotificationId} to {Email}, Status: {Status}",
                    notification.NotificationId, recipientUser.Email, log.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email notification {NotificationId}", notification.NotificationId);

                // Log failure
                var log = new EmailNotificationLog
                {
                    LogId = Guid.NewGuid(),
                    NotificationId = notification.NotificationId,
                    RecipientEmail = recipientUser.Email,
                    EventType = notification.EventType,
                    Subject = "Failed to generate",
                    SentAt = DateTimeOffset.UtcNow,
                    Status = "FAILED",
                    ErrorMsg = ex.Message,
                    RetryCount = 0
                };
                log.MarkAsFailed(ex.Message);

                _db.EmailNotificationLogs.Add(log);
                await _db.SaveChangesAsync();
            }
        }

        public async Task RetryFailedEmailsAsync()
        {
            var failedLogs = await _db.EmailNotificationLogs
                .Where(log => log.Status == "FAILED" && log.RetryCount < 3)
                .Include(log => log.Notification)
                    .ThenInclude(n => n.RecipientUser)
                .ToListAsync();

            foreach (var log in failedLogs)
            {
                if (log.Notification?.RecipientUser == null)
                    continue;

                try
                {
                    log.IncrementRetry();
                    await SendEmailNotificationAsync(log.Notification, log.Notification.RecipientUser);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Retry failed for email log {LogId}", log.LogId);
                }
            }
        }

        public async Task LogEmailDeliveryAsync(EmailNotificationLog log)
        {
            _db.EmailNotificationLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}

