using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
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

        public async Task SendEmailNotificationAsync(UserNotification notification, User recipientUser)
        {
            try
            {
                // Get template for notification type - use a default template for now
                var template = new DefaultNotificationTemplate();

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
                    EventType = notification.NotificationType?.TypeName ?? "Unknown",
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

                // Note: UserNotification doesn't have DeliveryStatus property
                // Delivery status is tracked in EmailNotificationLog

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
                    EventType = notification.NotificationType?.TypeName ?? "Unknown",
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
                    .ThenInclude(n => n.User)
                .ToListAsync();

            foreach (var log in failedLogs)
            {
                if (log.Notification?.User == null)
                    continue;

                try
                {
                    log.IncrementRetry();
                    await SendEmailNotificationAsync(log.Notification, log.Notification.User);
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

        public async Task<EmailResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending email to {Email} with subject: {Subject}", to, subject);

                var emailResult = await _fluentEmail
                    .To(to)
                    .Subject(subject)
                    .Body(body, true)
                    .SendAsync();

                if (emailResult.Successful)
                {
                    _logger.LogInformation("Successfully sent email to {Email}", to);
                    return new EmailResult
                    {
                        IsSuccess = true,
                        MessageId = emailResult.MessageId ?? Guid.NewGuid().ToString()
                    };
                }
                else
                {
                    var errorMessage = string.Join(", ", emailResult.ErrorMessages);
                    _logger.LogWarning("Failed to send email to {Email}: {Error}", to, errorMessage);
                    return new EmailResult
                    {
                        IsSuccess = false,
                        ErrorMessage = errorMessage,
                        ErrorDetails = errorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", to);
                return new EmailResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    ErrorDetails = ex.ToString()
                };
            }
        }

        public async Task<EmailResult> SendEmailAsync(List<string> to, string subject, string body, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending email to {Count} recipients with subject: {Subject}", to.Count, subject);

                var tasks = to.Select(email => SendEmailAsync(email, subject, body, cancellationToken));
                var results = await Task.WhenAll(tasks);

                var successCount = results.Count(r => r.IsSuccess);
                var failureCount = results.Length - successCount;

                if (failureCount == 0)
                {
                    _logger.LogInformation("Successfully sent email to all {Count} recipients", to.Count);
                    return new EmailResult
                    {
                        IsSuccess = true,
                        MessageId = $"bulk_{Guid.NewGuid():N}"
                    };
                }
                else
                {
                    var errorMessage = $"Sent to {successCount}/{to.Count} recipients. {failureCount} failed.";
                    _logger.LogWarning("Partial success sending bulk email: {Message}", errorMessage);
                    return new EmailResult
                    {
                        IsSuccess = successCount > 0,
                        MessageId = $"bulk_{Guid.NewGuid():N}",
                        ErrorMessage = errorMessage,
                        ErrorDetails = string.Join("; ", results.Where(r => !r.IsSuccess).Select(r => r.ErrorMessage))
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk email");
                return new EmailResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    ErrorDetails = ex.ToString()
                };
            }
        }
    }
}

