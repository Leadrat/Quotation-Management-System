using System;
using System.Collections.Generic;
using System.Text.Json;
using CRM.Domain.Entities;

namespace CRM.Application.Notifications.Services
{
    public abstract class BaseNotificationTemplate : INotificationTemplate
    {
        public abstract string GetSubject(UserNotification notification);
        public abstract string GetBody(UserNotification notification);

        protected string ReplacePlaceholders(string template, UserNotification notification, Dictionary<string, string>? additionalPlaceholders = null)
        {
            var result = template;
            
            // Note: UserNotification doesn't have Meta property, so we skip meta parsing for now

            // Replace standard placeholders
            result = result.Replace("{Message}", notification.Message);
            result = result.Replace("{Title}", notification.Title);
            result = result.Replace("{RelatedEntityType}", notification.RelatedEntityType ?? "");
            result = result.Replace("{RelatedEntityId}", notification.RelatedEntityId?.ToString() ?? "");

            // Meta placeholders would be handled here if UserNotification had a Meta property

            // Replace additional placeholders
            if (additionalPlaceholders != null)
            {
                foreach (var kvp in additionalPlaceholders)
                {
                    result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
            }

            return result;
        }
    }

    public class DefaultNotificationTemplate : BaseNotificationTemplate
    {
        public override string GetSubject(UserNotification notification)
        {
            return $"Notification: {notification.NotificationType?.TypeName ?? "Unknown"}";
        }

        public override string GetBody(UserNotification notification)
        {
            return $@"
                <h2>Notification</h2>
                <p>{notification.Message}</p>
                <p><strong>Type:</strong> {notification.NotificationType?.TypeName ?? "Unknown"}</p>
                <p><strong>Related Entity:</strong> {notification.RelatedEntityType ?? "None"} ({notification.RelatedEntityId?.ToString() ?? "N/A"})</p>
            ";
        }
    }
}

