using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Dispatch service for in-app notifications
/// </summary>
public class InAppNotificationDispatchService : IChannelDispatchService
{
    private readonly IAppDbContext _context;
    private readonly CRM.Application.Common.Interfaces.IRealTimeNotificationService _realTimeService;
    private readonly ILogger<InAppNotificationDispatchService> _logger;

    public NotificationChannel Channel => NotificationChannel.InApp;

    public InAppNotificationDispatchService(
        IAppDbContext context,
        CRM.Application.Common.Interfaces.IRealTimeNotificationService realTimeService,
        ILogger<InAppNotificationDispatchService> logger)
    {
        _context = context;
        _realTimeService = realTimeService;
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
            RetryCount = 0
        };

        try
        {
            _logger.LogInformation("Dispatching in-app notification {NotificationId} to user {UserId}", 
                notification.NotificationId, notification.UserId);

            // For in-app notifications, we just need to send via real-time service
            // The notification is already stored in the database
            await _realTimeService.SendNotificationToUserAsync(
                notification.UserId,
                notification);

            attempt.Status = DispatchStatus.Delivered;
            attempt.CompletedAt = DateTimeOffset.UtcNow;
            attempt.ExternalReference = notification.NotificationId.ToString(); // Use notification ID as external ID

            _logger.LogInformation("Successfully dispatched in-app notification {NotificationId}", notification.NotificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch in-app notification {NotificationId}", notification.NotificationId);
            
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
        // In-app notifications can always be dispatched if user exists
        return await _context.Users
            .AnyAsync(u => u.UserId == notification.UserId && u.IsActive);
    }

    public async Task<bool> IsChannelEnabledAsync()
    {
        // In-app notifications are always enabled
        return true;
    }
}