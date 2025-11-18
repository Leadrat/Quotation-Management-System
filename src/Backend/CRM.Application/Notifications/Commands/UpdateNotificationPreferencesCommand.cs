using System;
using System.Collections.Generic;
using CRM.Application.Notifications.Dtos;

namespace CRM.Application.Notifications.Commands
{
    public class UpdateNotificationPreferencesCommand
    {
        public Guid UserId { get; set; }
        public Dictionary<string, Dictionary<string, bool>> Preferences { get; set; } = new Dictionary<string, Dictionary<string, bool>>();
    }
}

