using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for synchronizing notification status and handling missed notifications
/// </summary>
public class NotificationSynchronizationService : INotificationSynchronizationService
{
    private readonly IAppDbContext _context;
    private readonly IRealTimeNotificationService _realTimeService;
    private readonly ILogger<NotificationSynchronizationService> _logger;

    public NotificationSynchronizationService(
        IAppDbContext context,
        IRealTimeNotificationService realTimeService,
        ILogger<NotificationSynchronizationService> logger)
    {
        _context = context;
        _realTimeService = realTimeService;
        _logger = logger;
    }

    public async Task SyncNotificationReadStatusAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Syncing read status for notification {NotificationId} and user {UserId}",
                notificationId, userId);

            // Update the notification read status in database
            var notification = await _context.UserNotifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && 
                                        n.UserId == userId, 
                                   cancellationToken);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTimeOffset.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                // Broadcast the read status update in real-time
                await _realTimeService.NotifyNotificationReadAsync(userId, notificationId, cancellationToken);

                // Update unread count
                await UpdateUnreadCountAsync(userId, cancellationToken);

                _logger.LogDebug("Successfully synced read status for notification {NotificationId}", notificationId);
            }
            else if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found for user {UserId}", notificationId, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing read status for notification {NotificationId} and user {UserId}",
                notificationId, userId);
            throw;
        }
    }

    public async Task SyncMissedNotificationsAsync(
        Guid userId,
        DateTime lastSyncTime,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Syncing missed notifications for user {UserId} since {LastSyncTime}",
                userId, lastSyncTime);

            // Get notifications created after the last sync time
            var missedNotifications = await _context.UserNotifications
                .Where(n => n.UserId == userId && 
                           n.CreatedAt > lastSyncTime)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync(cancellationToken);

            if (missedNotifications.Any())
            {
                _logger.LogInformation("Found {Count} missed notifications for user {UserId}",
                    missedNotifications.Count, userId);

                // Send each missed notification via real-time service
                foreach (var notification in missedNotifications)
                {
                    await _realTimeService.SendNotificationToUserAsync(userId, notification, cancellationToken);
                }

                // Update unread count
                await UpdateUnreadCountAsync(userId, cancellationToken);

                _logger.LogDebug("Successfully synced {Count} missed notifications for user {UserId}",
                    missedNotifications.Count, userId);
            }
            else
            {
                _logger.LogDebug("No missed notifications found for user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing missed notifications for user {UserId}", userId);
            throw;
        }
    }

    public async Task UpdateUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current unread count
            var unreadCount = await _context.UserNotifications
                .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);

            // Broadcast the updated count
            await _realTimeService.SendUnreadCountUpdateAsync(userId, unreadCount, cancellationToken);

            _logger.LogDebug("Updated unread count for user {UserId}: {UnreadCount}", userId, unreadCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unread count for user {UserId}", userId);
            throw;
        }
    }

    public async Task BulkMarkAsReadAsync(
        Guid userId,
        List<Guid> notificationIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Bulk marking {Count} notifications as read for user {UserId}",
                notificationIds.Count, userId);

            // Update multiple notifications at once
            var notifications = await _context.UserNotifications
                .Where(n => n.UserId == userId && 
                           notificationIds.Contains(n.NotificationId) && 
                           !n.IsRead)
                .ToListAsync(cancellationToken);

            if (notifications.Any())
            {
                var readAt = DateTimeOffset.UtcNow;
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = readAt;
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Broadcast read status updates for each notification
                foreach (var notificationId in notificationIds)
                {
                    await _realTimeService.NotifyNotificationReadAsync(userId, notificationId, cancellationToken);
                }

                // Update unread count
                await UpdateUnreadCountAsync(userId, cancellationToken);

                _logger.LogInformation("Successfully marked {Count} notifications as read for user {UserId}",
                    notifications.Count, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk marking notifications as read for user {UserId}", userId);
            throw;
        }
    }

    public async Task HandleUserReconnectionAsync(
        Guid userId,
        DateTime? lastConnectionTime = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Handling user reconnection for user {UserId}", userId);

            // If we have a last connection time, sync missed notifications
            if (lastConnectionTime.HasValue)
            {
                await SyncMissedNotificationsAsync(userId, lastConnectionTime.Value, cancellationToken);
            }
            else
            {
                // If no last connection time, just update the unread count
                await UpdateUnreadCountAsync(userId, cancellationToken);
            }

            _logger.LogDebug("Successfully handled reconnection for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling user reconnection for user {UserId}", userId);
            throw;
        }
    }

    public async Task<NotificationSyncStatus> GetSyncStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var status = new NotificationSyncStatus
            {
                UserId = userId,
                LastSyncTime = DateTime.UtcNow,
                UnreadCount = await _context.UserNotifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken),
                TotalNotifications = await _context.UserNotifications
                    .CountAsync(n => n.UserId == userId, cancellationToken),
                LastNotificationTime = await _context.UserNotifications
                    .Where(n => n.UserId == userId)
                    .MaxAsync(n => (DateTime?)n.CreatedAt.DateTime, cancellationToken)
            };

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sync status for user {UserId}", userId);
            throw;
        }
    }
}

/// <summary>
/// Interface for notification synchronization service
/// </summary>
public interface INotificationSynchronizationService
{
    /// <summary>
    /// Syncs notification read status and broadcasts update
    /// </summary>
    Task SyncNotificationReadStatusAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs missed notifications since last connection
    /// </summary>
    Task SyncMissedNotificationsAsync(
        Guid userId,
        DateTime lastSyncTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates and broadcasts unread count for user
    /// </summary>
    Task UpdateUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk marks notifications as read and syncs status
    /// </summary>
    Task BulkMarkAsReadAsync(
        Guid userId,
        List<Guid> notificationIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles user reconnection and syncs missed notifications
    /// </summary>
    Task HandleUserReconnectionAsync(
        Guid userId,
        DateTime? lastConnectionTime = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current sync status for user
    /// </summary>
    Task<NotificationSyncStatus> GetSyncStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Status information for notification synchronization
/// </summary>
public class NotificationSyncStatus
{
    public Guid UserId { get; set; }
    public DateTime LastSyncTime { get; set; }
    public int UnreadCount { get; set; }
    public int TotalNotifications { get; set; }
    public DateTime? LastNotificationTime { get; set; }
}