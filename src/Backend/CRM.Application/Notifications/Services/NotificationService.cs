using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IAppDbContext _db;
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly IRealTimeNotificationService? _realTimeNotificationService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IAppDbContext db,
            IEmailNotificationService emailNotificationService,
            IRealTimeNotificationService? realTimeNotificationService,
            ILogger<NotificationService> logger)
        {
            _db = db;
            _emailNotificationService = emailNotificationService;
            _realTimeNotificationService = realTimeNotificationService;
            _logger = logger;
        }

        public async Task<Guid> PublishNotificationAsync(
            NotificationEventType eventType,
            string relatedEntityType,
            Guid relatedEntityId,
            Guid recipientUserId,
            string message,
            Dictionary<string, object>? meta = null,
            List<NotificationChannel>? channels = null)
        {
            // Check for duplicate (same event, same entity, same user within last 5 minutes)
            var fiveMinutesAgo = DateTimeOffset.UtcNow.AddMinutes(-5);
            var duplicate = await _db.Notifications
                .AnyAsync(n => n.RecipientUserId == recipientUserId &&
                               n.RelatedEntityType == relatedEntityType &&
                               n.RelatedEntityId == relatedEntityId &&
                               n.EventType == eventType.ToString() &&
                               n.CreatedAt > fiveMinutesAgo);

            if (duplicate)
            {
                _logger.LogWarning("Duplicate notification prevented for user {UserId}, event {EventType}, entity {EntityType}/{EntityId}",
                    recipientUserId, eventType, relatedEntityType, relatedEntityId);
                return Guid.Empty;
            }

            // Check user preferences
            channels = channels ?? new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email };
            var enabledChannels = new List<NotificationChannel>();

            foreach (var channel in channels)
            {
                if (await ShouldSendNotificationAsync(recipientUserId, eventType, channel))
                {
                    enabledChannels.Add(channel);
                }
            }

            if (enabledChannels.Count == 0)
            {
                _logger.LogInformation("Notification suppressed for user {UserId}, event {EventType} - all channels disabled",
                    recipientUserId, eventType);
                return Guid.Empty;
            }

            // Create notification
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                RecipientUserId = recipientUserId,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                EventType = eventType.ToString(),
                Message = message,
                IsRead = false,
                IsArchived = false,
                DeliveredChannels = string.Join(",", enabledChannels.Select(c => c.ToString().ToLower())),
                DeliveryStatus = "SENT",
                CreatedAt = DateTimeOffset.UtcNow,
                Meta = meta != null ? System.Text.Json.JsonSerializer.Serialize(meta) : null
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            // Send via enabled channels
            var recipientUser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == recipientUserId);
            if (recipientUser == null)
            {
                _logger.LogWarning("Recipient user {UserId} not found for notification", recipientUserId);
                return notification.NotificationId;
            }

            if (enabledChannels.Contains(NotificationChannel.Email))
            {
                try
                {
                    await _emailNotificationService.SendEmailNotificationAsync(notification, recipientUser);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email notification {NotificationId}", notification.NotificationId);
                }
            }

            if (enabledChannels.Contains(NotificationChannel.InApp) && _realTimeNotificationService != null)
            {
                try
                {
                    var dto = new NotificationDto
                    {
                        NotificationId = notification.NotificationId,
                        RecipientUserId = notification.RecipientUserId,
                        RelatedEntityType = notification.RelatedEntityType,
                        RelatedEntityId = notification.RelatedEntityId,
                        EventType = notification.EventType,
                        Message = notification.Message,
                        IsRead = notification.IsRead,
                        IsArchived = notification.IsArchived,
                        DeliveredChannels = notification.DeliveredChannels,
                        DeliveryStatus = notification.DeliveryStatus,
                        CreatedAt = notification.CreatedAt,
                        ReadAt = notification.ReadAt,
                        ArchivedAt = notification.ArchivedAt,
                        Meta = notification.Meta
                    };
                    await _realTimeNotificationService.SendToUserAsync(recipientUserId, dto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send real-time notification {NotificationId}", notification.NotificationId);
                }
            }

            _logger.LogInformation("Notification {NotificationId} published for user {UserId}, event {EventType}",
                notification.NotificationId, recipientUserId, eventType);

            return notification.NotificationId;
        }

        public async Task<List<Guid>> PublishBulkNotificationsAsync(
            NotificationEventType eventType,
            string relatedEntityType,
            Guid relatedEntityId,
            List<Guid> recipientUserIds,
            string message,
            Dictionary<string, object>? meta = null,
            List<NotificationChannel>? channels = null)
        {
            var notificationIds = new List<Guid>();

            foreach (var userId in recipientUserIds)
            {
                try
                {
                    var id = await PublishNotificationAsync(eventType, relatedEntityType, relatedEntityId, userId, message, meta, channels);
                    if (id != Guid.Empty)
                    {
                        notificationIds.Add(id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish notification to user {UserId}", userId);
                }
            }

            return notificationIds;
        }

        public async Task<NotificationPreferencesDto?> GetUserPreferencesAsync(Guid userId)
        {
            var preference = await _db.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (preference == null)
                return null;

            return new NotificationPreferencesDto
            {
                UserId = preference.UserId,
                Preferences = preference.GetPreferences()
            };
        }

        public async Task<bool> ShouldSendNotificationAsync(Guid userId, NotificationEventType eventType, NotificationChannel channel)
        {
            var preference = await _db.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (preference == null)
            {
                // Default: in-app enabled, others disabled
                return channel == NotificationChannel.InApp;
            }

            var eventTypeStr = eventType.ToString();
            var channelStr = channel.ToString().ToLower();

            // Check if muted
            if (preference.IsMuted(eventTypeStr))
                return false;

            // Check if channel enabled
            return preference.IsChannelEnabled(eventTypeStr, channelStr);
        }
    }
}

