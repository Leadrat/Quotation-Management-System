using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Dispatch service for email notifications
/// </summary>
public class EmailNotificationDispatchService : IChannelDispatchService
{
    private readonly IAppDbContext _context;
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<EmailNotificationDispatchService> _logger;

    public NotificationChannel Channel => NotificationChannel.Email;

    public EmailNotificationDispatchService(
        IAppDbContext context,
        IEmailNotificationService emailService,
        ILogger<EmailNotificationDispatchService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<NotificationDispatchAttempt> DispatchAsync(UserNotification notification)
    {
        var attempt = new NotificationDispatchAttempt
        {
            NotificationId = notification.NotificationId,
            Channel = Channel,
            Status = DispatchStatus.Pending,
            AttemptedAt = DateTimeOffset.UtcNow,
            AttemptNumber = 1
        };

        try
        {
            _logger.LogInformation("Dispatching email notification {NotificationId} to user {UserId}", 
                notification.NotificationId, notification.UserId);

            // Get user email address
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == notification.UserId);

            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                throw new InvalidOperationException($"User {notification.UserId} not found or has no email address");
            }

            // Send email using the email service
            var emailResult = await _emailService.SendEmailAsync(
                user.Email,
                notification.Title,
                notification.Message);

            if (emailResult.IsSuccess)
            {
                attempt.Status = DispatchStatus.Delivered;
                attempt.DeliveredAt = DateTimeOffset.UtcNow;
                attempt.ExternalId = emailResult.MessageId; // Use email service message ID
                
                _logger.LogInformation("Successfully dispatched email notification {NotificationId} with message ID {MessageId}", 
                    notification.NotificationId, emailResult.MessageId);
            }
            else
            {
                attempt.Status = DispatchStatus.Failed;
                attempt.ErrorMessage = emailResult.ErrorMessage;
                attempt.ErrorDetails = emailResult.ErrorDetails;
                
                _logger.LogWarning("Failed to dispatch email notification {NotificationId}: {Error}", 
                    notification.NotificationId, emailResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching email notification {NotificationId}", notification.NotificationId);
            
            attempt.Status = DispatchStatus.Failed;
            attempt.ErrorMessage = ex.Message;
            attempt.ErrorDetails = ex.ToString();
        }

        _context.NotificationDispatchAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        return attempt;
    }

    public async Task<bool> CanDispatchAsync(UserNotification notification)
    {
        // Email notifications can be dispatched if user exists and has valid email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == notification.UserId && u.IsActive);

        return user != null && !string.IsNullOrEmpty(user.Email) && IsValidEmail(user.Email);
    }

    public async Task<bool> IsChannelEnabledAsync()
    {
        // Check if email notifications are enabled in system configuration
        // For now, return true - this could be configurable
        return true;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);
            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }
}