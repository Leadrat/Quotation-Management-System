using CRM.Domain.Entities;

namespace CRM.Application.Common.Interfaces;

/// <summary>
/// Service for sending real-time notifications via WebSocket/SignalR
/// </summary>
public interface IRealTimeNotificationService
{
    /// <summary>
    /// Sends a notification to a specific user via real-time connection
    /// </summary>
    Task SendNotificationToUserAsync(
        Guid userId,
        UserNotification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to multiple users
    /// </summary>
    Task SendNotificationToUsersAsync(
        List<Guid> userIds,
        UserNotification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to all connected users
    /// </summary>
    Task SendNotificationToAllUsersAsync(
        UserNotification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies user about notification read status change
    /// </summary>
    Task NotifyNotificationReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends updated unread count to user
    /// </summary>
    Task SendUnreadCountUpdateAsync(
        Guid userId,
        int unreadCount,
        CancellationToken cancellationToken = default);
}