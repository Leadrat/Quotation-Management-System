using CRM.Application.Common.Interfaces;
using CRM.Domain.Entities;
using CRM.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for sending real-time notifications via SignalR
/// </summary>
public class RealTimeNotificationService : IRealTimeNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<RealTimeNotificationService> _logger;

    public RealTimeNotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<RealTimeNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(
        Guid userId,
        UserNotification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending real-time notification {NotificationId} to user {UserId}", 
                notification.Id, userId);

            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    Id = notification.NotificationId,
                    notification.Title,
                    notification.Message,
                    Type = notification.EventType,
                    notification.Priority,
                    notification.CreatedAt,
                    notification.IsRead
                }, cancellationToken);

            _logger.LogDebug("Successfully sent real-time notification {NotificationId} to user {UserId}", 
                notification.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending real-time notification {NotificationId} to user {UserId}", 
                notification.Id, userId);
            throw;
        }
    }

    public async Task SendNotificationToUsersAsync(
        List<Guid> userIds,
        UserNotification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending real-time notification {NotificationId} to {UserCount} users", 
                notification.Id, userIds.Count);

            var userIdStrings = userIds.Select(id => id.ToString()).ToList();
            
            await _hubContext.Clients.Users(userIdStrings)
                .SendAsync("ReceiveNotification", new
                {
                    Id = notification.NotificationId,
                    notification.Title,
                    notification.Message,
                    Type = notification.EventType,
                    notification.Priority,
                    notification.CreatedAt,
                    notification.IsRead
                }, cancellationToken);

            _logger.LogDebug("Successfully sent real-time notification {NotificationId} to {UserCount} users", 
                notification.Id, userIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending real-time notification {NotificationId} to multiple users", 
                notification.Id);
            throw;
        }
    }

    public async Task SendNotificationToAllUsersAsync(
        UserNotification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending real-time notification {NotificationId} to all users", 
                notification.Id);

            await _hubContext.Clients.All
                .SendAsync("ReceiveNotification", new
                {
                    Id = notification.NotificationId,
                    notification.Title,
                    notification.Message,
                    Type = notification.EventType,
                    notification.Priority,
                    notification.CreatedAt,
                    notification.IsRead
                }, cancellationToken);

            _logger.LogDebug("Successfully sent real-time notification {NotificationId} to all users", 
                notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending real-time notification {NotificationId} to all users", 
                notification.Id);
            throw;
        }
    }

    public async Task NotifyNotificationReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Notifying user {UserId} about notification {NotificationId} read status", 
                userId, notificationId);

            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("NotificationRead", new { NotificationId = notificationId }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user {UserId} about notification {NotificationId} read status", 
                userId, notificationId);
            throw;
        }
    }

    public async Task SendUnreadCountUpdateAsync(
        Guid userId,
        int unreadCount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Sending unread count update to user {UserId}: {UnreadCount}", 
                userId, unreadCount);

            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("UnreadCountUpdate", new { UnreadCount = unreadCount }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending unread count update to user {UserId}", userId);
            throw;
        }
    }

}