using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace CRM.Domain.Entities
{
    [Table("NotificationPreferences")]
    public class NotificationPreference
    {
        public Guid UserId { get; set; }
        public string PreferenceData { get; set; } = "{}"; // JSON string
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation property
        public virtual User User { get; set; } = null!;

        // Domain methods
        public Dictionary<string, Dictionary<string, bool>> GetPreferences()
        {
            if (string.IsNullOrWhiteSpace(PreferenceData) || PreferenceData == "{}")
                return new Dictionary<string, Dictionary<string, bool>>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(PreferenceData) 
                    ?? new Dictionary<string, Dictionary<string, bool>>();
            }
            catch
            {
                return new Dictionary<string, Dictionary<string, bool>>();
            }
        }

        public void UpdatePreferences(Dictionary<string, Dictionary<string, bool>> preferences)
        {
            PreferenceData = JsonSerializer.Serialize(preferences);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public bool IsMuted(string eventType)
        {
            var prefs = GetPreferences();
            if (prefs.TryGetValue(eventType, out var eventPrefs))
            {
                return eventPrefs.TryGetValue("muted", out var muted) && muted;
            }
            return false;
        }

        public bool IsChannelEnabled(string eventType, string channel)
        {
            if (IsMuted(eventType))
                return false;

            var prefs = GetPreferences();
            if (prefs.TryGetValue(eventType, out var eventPrefs))
            {
                return eventPrefs.TryGetValue(channel, out var enabled) && enabled;
            }
            // Default: in-app enabled, others disabled
            return channel == "inApp";
        }
    }
}

