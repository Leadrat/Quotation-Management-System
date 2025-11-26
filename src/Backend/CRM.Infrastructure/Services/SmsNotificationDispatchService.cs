using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Dispatch service for SMS notifications
/// </summary>
public class SmsNotificationDispatchService : IChannelDispatchService
{
    private readonly IAppDbContext _context;
    private readonly ISmsService _smsService;
    private readonly ILogger<SmsNotificationDispatchService> _logger;
    private const int SMS_CHARACTER_LIMIT = 160;

    public NotificationChannel Channel => NotificationChannel.SMS;

    public SmsNotificationDispatchService(
        IAppDbContext context,
        ISmsService smsService,
        ILogger<SmsNotificationDispatchService> logger)
    {
        _context = context;
        _smsService = smsService;
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
            _logger.LogInformation("Dispatching SMS notification {NotificationId} to user {UserId}", 
                notification.NotificationId, notification.UserId);

            // Get user phone number
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == notification.UserId);

            if (user == null || string.IsNullOrEmpty(user.Mobile))
            {
                throw new InvalidOperationException($"User {notification.UserId} not found or has no phone number");
            }

            // Format message for SMS (remove HTML, enforce character limit)
            var smsMessage = FormatMessageForSms(notification.Message);

            // Send SMS using the SMS service
            var smsResult = await _smsService.SendSmsAsync(
                user.Mobile,
                smsMessage);

            if (smsResult.IsSuccess)
            {
                attempt.Status = DispatchStatus.Delivered;
                attempt.DeliveredAt = DateTimeOffset.UtcNow;
                attempt.ExternalId = smsResult.MessageId; // Use SMS service message ID
                
                _logger.LogInformation("Successfully dispatched SMS notification {NotificationId} with message ID {MessageId}", 
                    notification.NotificationId, smsResult.MessageId);
            }
            else
            {
                attempt.Status = DispatchStatus.Failed;
                attempt.ErrorMessage = smsResult.ErrorMessage;
                attempt.ErrorDetails = smsResult.ErrorDetails;
                
                _logger.LogWarning("Failed to dispatch SMS notification {NotificationId}: {Error}", 
                    notification.NotificationId, smsResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching SMS notification {NotificationId}", notification.NotificationId);
            
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
        // SMS notifications can be dispatched if user exists and has valid phone number
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == notification.UserId && u.IsActive);

        return user != null && !string.IsNullOrEmpty(user.Mobile) && IsValidPhoneNumber(user.Mobile);
    }

    public async Task<bool> IsChannelEnabledAsync()
    {
        // Check if SMS notifications are enabled in system configuration
        // For now, return true - this could be configurable
        return true;
    }

    private static string FormatMessageForSms(string message)
    {
        // Remove HTML tags
        var plainText = Regex.Replace(message, "<.*?>", string.Empty);
        
        // Decode HTML entities
        plainText = System.Net.WebUtility.HtmlDecode(plainText);
        
        // Normalize whitespace
        plainText = Regex.Replace(plainText, @"\s+", " ").Trim();
        
        // Enforce character limit
        if (plainText.Length > SMS_CHARACTER_LIMIT)
        {
            plainText = plainText.Substring(0, SMS_CHARACTER_LIMIT - 3) + "...";
        }
        
        return plainText;
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        // Basic phone number validation - in reality you'd use a more sophisticated validation
        var phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$");
        return phoneRegex.IsMatch(phoneNumber.Replace(" ", "").Replace("-", ""));
    }
}