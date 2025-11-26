using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CRM.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time notification delivery
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(
        IAppDbContext context,
        ICurrentUserService currentUserService,
        ILogger<NotificationHub> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                // Add user to their personal group
                var userGroupName = GetUserGroupName(userId.Value);
                await Groups.AddToGroupAsync(Context.ConnectionId, userGroupName);

                // Add user to role-based groups
                await AddToRoleGroupsAsync(userId.Value);

                _logger.LogInformation("User {UserId} connected to notification hub with connection {ConnectionId}", 
                    userId.Value, Context.ConnectionId);

                // Send any missed notifications
                await SendMissedNotificationsAsync(userId.Value);
            }
            else
            {
                _logger.LogWarning("Anonymous user attempted to connect to notification hub");
                Context.Abort();
                return;
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during client connection to notification hub");
            throw;
        }
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                _logger.LogInformation("User {UserId} disconnected from notification hub with connection {ConnectionId}", 
                    userId.Value, Context.ConnectionId);

                if (exception != null)
                {
                    _logger.LogWarning(exception, "User {UserId} disconnected due to exception", userId.Value);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during client disconnection from notification hub");
        }
    }

    /// <summary>
    /// Join a specific user group (called by client)
    /// </summary>
    public async Task JoinUserGroup()
    {
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            var userGroupName = GetUserGroupName(userId.Value);
            await Groups.AddToGroupAsync(Context.ConnectionId, userGroupName);
            
            _logger.LogDebug("User {UserId} joined group {GroupName}", userId.Value, userGroupName);
        }
    }

    /// <summary>
    /// Leave a specific user group (called by client)
    /// </summary>
    public async Task LeaveUserGroup()
    {
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            var userGroupName = GetUserGroupName(userId.Value);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userGroupName);
            
            _logger.LogDebug("User {UserId} left group {GroupName}", userId.Value, userGroupName);
        }
    }

    /// <summary>
    /// Mark a notification as read (called by client)
    /// </summary>
    public async Task MarkNotificationAsRead(Guid notificationId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var notification = await _context.UserNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId.Value);

            if (notification == null)
            {
                await Clients.Caller.SendAsync("Error", "Notification not found");
                return;
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTimeOffset.UtcNow;
                await _context.SaveChangesAsync();

                // Notify the user that the notification was marked as read
                await Clients.Caller.SendAsync("NotificationRead", notificationId);

                // Update unread count
                var unreadCount = await _context.UserNotifications
                    .CountAsync(n => n.UserId == userId.Value && !n.IsRead);

                await Clients.Caller.SendAsync("UnreadCountUpdated", unreadCount);

                _logger.LogDebug("Notification {NotificationId} marked as read by user {UserId}", 
                    notificationId, userId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
            await Clients.Caller.SendAsync("Error", "Failed to mark notification as read");
        }
    }

    /// <summary>
    /// Get current unread count (called by client)
    /// </summary>
    public async Task GetUnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            var unreadCount = await _context.UserNotifications
                .CountAsync(n => n.UserId == userId.Value && !n.IsRead);

            await Clients.Caller.SendAsync("UnreadCountUpdated", unreadCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user");
            await Clients.Caller.SendAsync("Error", "Failed to get unread count");
        }
    }

    /// <summary>
    /// Request missed notifications since last connection (called by client)
    /// </summary>
    public async Task RequestMissedNotifications(DateTime? lastConnectionTime = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            await SendMissedNotificationsAsync(userId.Value, lastConnectionTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending missed notifications to user");
            await Clients.Caller.SendAsync("Error", "Failed to get missed notifications");
        }
    }

    /// <summary>
    /// Send a notification to a specific user
    /// </summary>
    public async Task SendNotificationToUserAsync(Guid userId, object notification)
    {
        try
        {
            var userGroupName = GetUserGroupName(userId);
            await Clients.Group(userGroupName).SendAsync("NotificationReceived", notification);
            
            _logger.LogDebug("Notification sent to user {UserId} via SignalR", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId} via SignalR", userId);
        }
    }

    /// <summary>
    /// Send a notification to multiple users
    /// </summary>
    public async Task SendNotificationToUsersAsync(List<Guid> userIds, object notification)
    {
        try
        {
            var groupNames = userIds.Select(GetUserGroupName).ToList();
            await Clients.Groups(groupNames).SendAsync("NotificationReceived", notification);
            
            _logger.LogDebug("Notification sent to {UserCount} users via SignalR", userIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to multiple users via SignalR");
        }
    }

    /// <summary>
    /// Send a notification to all users in a role
    /// </summary>
    public async Task SendNotificationToRoleAsync(string roleName, object notification)
    {
        try
        {
            var roleGroupName = GetRoleGroupName(roleName);
            await Clients.Group(roleGroupName).SendAsync("NotificationReceived", notification);
            
            _logger.LogDebug("Notification sent to role {RoleName} via SignalR", roleName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to role {RoleName} via SignalR", roleName);
        }
    }

    /// <summary>
    /// Broadcast a notification to all connected users
    /// </summary>
    public async Task BroadcastNotificationAsync(object notification)
    {
        try
        {
            await Clients.All.SendAsync("NotificationReceived", notification);
            
            _logger.LogDebug("Notification broadcasted to all users via SignalR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting notification via SignalR");
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    private static string GetUserGroupName(Guid userId)
    {
        return $"user_{userId}";
    }

    private static string GetRoleGroupName(string roleName)
    {
        return $"role_{roleName.ToLowerInvariant()}";
    }

    private async Task AddToRoleGroupsAsync(Guid userId)
    {
        try
        {
            var userRoles = await _context.Users
                .Where(u => u.UserId == userId)
                .SelectMany(u => u.UserRoles)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();

            foreach (var roleName in userRoles)
            {
                var roleGroupName = GetRoleGroupName(roleName);
                await Groups.AddToGroupAsync(Context.ConnectionId, roleGroupName);
            }

            _logger.LogDebug("User {UserId} added to {RoleCount} role groups", userId, userRoles.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {UserId} to role groups", userId);
        }
    }

    private async Task SendMissedNotificationsAsync(Guid userId, DateTime? lastConnectionTime = null)
    {
        try
        {
            var cutoffTime = lastConnectionTime ?? DateTime.UtcNow.AddHours(-24); // Default to last 24 hours

            var missedNotifications = await _context.UserNotifications
                .Where(n => n.UserId == userId && 
                           n.CreatedAt > cutoffTime && 
                           !n.IsRead)
                .OrderBy(n => n.CreatedAt)
                .Take(50) // Limit to prevent overwhelming the client
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.EventType,
                    n.Priority,
                    n.CreatedAt,
                    n.RelatedEntityType,
                    n.RelatedEntityId,
                    n.Metadata
                })
                .ToListAsync();

            if (missedNotifications.Any())
            {
                await Clients.Caller.SendAsync("MissedNotifications", missedNotifications);
                
                _logger.LogDebug("Sent {Count} missed notifications to user {UserId}", 
                    missedNotifications.Count, userId);
            }

            // Send current unread count
            var unreadCount = await _context.UserNotifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            await Clients.Caller.SendAsync("UnreadCountUpdated", unreadCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending missed notifications to user {UserId}", userId);
        }
    }
}