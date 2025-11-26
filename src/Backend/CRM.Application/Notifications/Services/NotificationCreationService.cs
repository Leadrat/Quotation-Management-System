using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Repositories;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Services;

public interface INotificationCreationService
{
    Task<UserNotification> CreateNotificationAsync(
        Guid userId,
        string title,
        string message,
        NotificationEventType eventType,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        Dictionary<string, object>? metadata = null,
        NotificationPriority priority = NotificationPriority.Normal);

    Task<List<UserNotification>> CreateBulkNotificationsAsync(
        List<Guid> userIds,
        string title,
        string message,
        NotificationEventType eventType,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        Dictionary<string, object>? metadata = null,
        NotificationPriority priority = NotificationPriority.Normal);

    Task<UserNotification> CreateTemplateBasedNotificationAsync(
        Guid userId,
        string templateKey,
        NotificationChannel channel,
        Dictionary<string, object> templateData,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        NotificationPriority priority = NotificationPriority.Normal);

    Task<List<UserNotification>> CreateBulkTemplateBasedNotificationsAsync(
        List<Guid> userIds,
        string templateKey,
        NotificationChannel channel,
        Dictionary<string, object> templateData,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        NotificationPriority priority = NotificationPriority.Normal);
}

public class NotificationCreationService : INotificationCreationService
{
    private readonly IAppDbContext _context;
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly INotificationTemplateService _templateService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<NotificationCreationService> _logger;

    public NotificationCreationService(
        IAppDbContext context,
        INotificationTemplateRepository templateRepository,
        INotificationTemplateService templateService,
        ICurrentUserService currentUserService,
        ILogger<NotificationCreationService> logger)
    {
        _context = context;
        _templateRepository = templateRepository;
        _templateService = templateService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UserNotification> CreateNotificationAsync(
        Guid userId,
        string title,
        string message,
        NotificationEventType eventType,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        Dictionary<string, object>? metadata = null,
        NotificationPriority priority = NotificationPriority.Normal)
    {
        // Authorization check
        await ValidateUserAccessAsync(userId);

        var notification = new UserNotification
        {
            UserId = userId,
            Title = title,
            Message = message,
            EventType = eventType.ToString(),
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            Priority = (int)priority,
            IsRead = false,
            Metadata = metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification created for user {UserId}: {Title}", userId, title);
        return notification;
    }

    public async Task<List<UserNotification>> CreateBulkNotificationsAsync(
        List<Guid> userIds,
        string title,
        string message,
        NotificationEventType eventType,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        Dictionary<string, object>? metadata = null,
        NotificationPriority priority = NotificationPriority.Normal)
    {
        // Authorization check for bulk operations
        await ValidateBulkUserAccessAsync(userIds);

        var notifications = new List<UserNotification>();
        var now = DateTimeOffset.UtcNow;

        foreach (var userId in userIds.Distinct())
        {
            var notification = new UserNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                EventType = eventType.ToString(),
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                Priority = (int)priority,
                IsRead = false,
                Metadata = metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : null,
                CreatedAt = now,
                UpdatedAt = now
            };

            notifications.Add(notification);
        }

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Bulk notifications created for {UserCount} users: {Title}", userIds.Count, title);
        return notifications;
    }

    public async Task<UserNotification> CreateTemplateBasedNotificationAsync(
        Guid userId,
        string templateKey,
        NotificationChannel channel,
        Dictionary<string, object> templateData,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        NotificationPriority priority = NotificationPriority.Normal)
    {
        // Authorization check
        await ValidateUserAccessAsync(userId);

        // Get template
        var template = await _templateRepository.GetByTemplateKeyAsync(templateKey);
        if (template == null)
        {
            throw new InvalidOperationException($"Template '{templateKey}' not found");
        }

        if (template.Channel != channel)
        {
            throw new InvalidOperationException($"Template '{templateKey}' is not configured for channel '{channel}'");
        }

        if (!template.IsActive)
        {
            throw new InvalidOperationException($"Template '{templateKey}' is not active");
        }

        // Render template
        var renderedContent = await _templateService.RenderTemplateAsync(template.TemplateKey, templateData, channel);

        var notification = new UserNotification
        {
            UserId = userId,
            Title = renderedContent.Subject ?? template.Name,
            Message = renderedContent.Body,
            EventType = template.EventType,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            Priority = (int)priority,
            IsRead = false,
            Metadata = templateData != null ? System.Text.Json.JsonSerializer.Serialize(templateData) : null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Template-based notification created for user {UserId} using template {TemplateKey}", userId, templateKey);
        return notification;
    }

    public async Task<List<UserNotification>> CreateBulkTemplateBasedNotificationsAsync(
        List<Guid> userIds,
        string templateKey,
        NotificationChannel channel,
        Dictionary<string, object> templateData,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        NotificationPriority priority = NotificationPriority.Normal)
    {
        // Authorization check for bulk operations
        await ValidateBulkUserAccessAsync(userIds);

        // Get template
        var template = await _templateRepository.GetByTemplateKeyAsync(templateKey);
        if (template == null)
        {
            throw new InvalidOperationException($"Template '{templateKey}' not found");
        }

        if (template.Channel != channel)
        {
            throw new InvalidOperationException($"Template '{templateKey}' is not configured for channel '{channel}'");
        }

        if (!template.IsActive)
        {
            throw new InvalidOperationException($"Template '{templateKey}' is not active");
        }

        // Render template once (assuming same data for all users)
        var renderedContent = await _templateService.RenderTemplateAsync(template.TemplateKey, templateData, channel);

        var notifications = new List<UserNotification>();
        var now = DateTimeOffset.UtcNow;

        foreach (var userId in userIds.Distinct())
        {
            var notification = new UserNotification
            {
                UserId = userId,
                Title = renderedContent.Subject ?? template.Name,
                Message = renderedContent.Body,
                EventType = template.EventType,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                Priority = (int)priority,
                IsRead = false,
                Metadata = templateData != null ? System.Text.Json.JsonSerializer.Serialize(templateData) : null,
                CreatedAt = now,
                UpdatedAt = now
            };

            notifications.Add(notification);
        }

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Bulk template-based notifications created for {UserCount} users using template {TemplateKey}", userIds.Count, templateKey);
        return notifications;
    }

    private async Task ValidateUserAccessAsync(Guid userId)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        // Check if current user can create notifications for the target user
        var currentUser = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == currentUserId.Value);

        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Current user not found");
        }

        // System administrators can create notifications for anyone
        if (currentUser.UserRoles.Any(ur => ur.Role.RoleName == "SystemAdministrator"))
        {
            return;
        }

        // Users can only create notifications for themselves
        if (currentUserId.Value != userId)
        {
            // Check if current user is a manager of the target user
            var targetUser = await _context.Users
                .Include(u => u.ReportingManager)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (targetUser?.ReportingManager?.UserId != currentUserId.Value)
            {
                throw new UnauthorizedAccessException("Insufficient permissions to create notification for this user");
            }
        }
    }

    private async Task ValidateBulkUserAccessAsync(List<Guid> userIds)
    {
        var currentUserId = _currentUserService.UserId;
        if (!currentUserId.HasValue)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var currentUser = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == currentUserId.Value);

        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("Current user not found");
        }

        // System administrators can create notifications for anyone
        if (currentUser.UserRoles.Any(ur => ur.Role.RoleName == "SystemAdministrator"))
        {
            return;
        }

        // For bulk operations, only allow if user is a manager or admin
        var hasManagerRole = currentUser.UserRoles.Any(ur => 
            ur.Role.RoleName == "Manager" || 
            ur.Role.RoleName == "Administrator" ||
            ur.Role.RoleName == "SystemAdministrator");

        if (!hasManagerRole)
        {
            throw new UnauthorizedAccessException("Insufficient permissions for bulk notification creation");
        }

        // Validate that all target users are within the current user's scope
        var managedUserIds = await _context.Users
            .Where(u => u.ReportingManager != null && u.ReportingManager.UserId == currentUserId.Value)
            .Select(u => u.UserId)
            .ToListAsync();

        managedUserIds.Add(currentUserId.Value); // User can always notify themselves

        var unauthorizedUserIds = userIds.Except(managedUserIds).ToList();
        if (unauthorizedUserIds.Any())
        {
            throw new UnauthorizedAccessException($"Insufficient permissions to create notifications for users: {string.Join(", ", unauthorizedUserIds)}");
        }
    }
}