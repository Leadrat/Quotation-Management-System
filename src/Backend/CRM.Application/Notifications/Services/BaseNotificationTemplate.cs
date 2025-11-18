using System;
using System.Collections.Generic;
using System.Text.Json;
using CRM.Domain.Entities;

namespace CRM.Application.Notifications.Services
{
    public abstract class BaseNotificationTemplate : INotificationTemplate
    {
        public abstract string GetSubject(Notification notification);
        public abstract string GetBody(Notification notification);

        protected string ReplacePlaceholders(string template, Notification notification, Dictionary<string, string>? additionalPlaceholders = null)
        {
            var result = template;
            
            // Parse meta if available
            Dictionary<string, object>? meta = null;
            if (!string.IsNullOrWhiteSpace(notification.Meta))
            {
                try
                {
                    meta = JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Meta);
                }
                catch { }
            }

            // Replace standard placeholders
            result = result.Replace("{Message}", notification.Message);
            result = result.Replace("{EventType}", notification.EventType);
            result = result.Replace("{RelatedEntityType}", notification.RelatedEntityType);
            result = result.Replace("{RelatedEntityId}", notification.RelatedEntityId.ToString());

            // Replace meta placeholders
            if (meta != null)
            {
                foreach (var kvp in meta)
                {
                    result = result.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
                }
            }

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
        public override string GetSubject(Notification notification)
        {
            return $"Notification: {notification.EventType}";
        }

        public override string GetBody(Notification notification)
        {
            return $@"
                <h2>Notification</h2>
                <p>{notification.Message}</p>
                <p><strong>Event Type:</strong> {notification.EventType}</p>
                <p><strong>Related Entity:</strong> {notification.RelatedEntityType} ({notification.RelatedEntityId})</p>
            ";
        }
    }
}

