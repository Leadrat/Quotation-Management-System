using System;
using System.Collections.Generic;

namespace CRM.Application.Notifications.Dtos
{
    public class NotificationPreferencesDto
    {
        public Guid UserId { get; set; }
        public Dictionary<string, Dictionary<string, bool>> Preferences { get; set; } = new Dictionary<string, Dictionary<string, bool>>();
    }
}

