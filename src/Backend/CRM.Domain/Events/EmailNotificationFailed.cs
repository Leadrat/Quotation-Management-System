using System;

namespace CRM.Domain.Events
{
    public class EmailNotificationFailed
    {
        public Guid LogId { get; set; }
        public string ErrorMsg { get; set; } = string.Empty;
        public int RetryCount { get; set; }
    }
}

