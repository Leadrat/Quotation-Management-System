using System.Collections.Generic;

namespace CRM.Application.Notifications.Dtos
{
    public class UpdateNotificationPreferencesRequest
    {
        public Dictionary<string, Dictionary<string, bool>> Preferences { get; set; } = new Dictionary<string, Dictionary<string, bool>>();
    }
}
