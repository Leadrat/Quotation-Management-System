using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.Notifications.Dtos;
using CRM.Domain.Enums;

namespace CRM.Application.Notifications.Services
{
    public interface INotificationService
    {
        Task<Guid> PublishNotificationAsync(
            NotificationEventType eventType,
            string relatedEntityType,
            Guid relatedEntityId,
            Guid recipientUserId,
            string message,
            Dictionary<string, object>? meta = null,
            List<NotificationChannel>? channels = null);

        Task<List<Guid>> PublishBulkNotificationsAsync(
            NotificationEventType eventType,
            string relatedEntityType,
            Guid relatedEntityId,
            List<Guid> recipientUserIds,
            string message,
            Dictionary<string, object>? meta = null,
            List<NotificationChannel>? channels = null);

        Task<NotificationPreferencesDto?> GetUserPreferencesAsync(Guid userId);

        Task<bool> ShouldSendNotificationAsync(Guid userId, NotificationEventType eventType, NotificationChannel channel);
    }
}

