using System;

namespace CRM.Domain.Events
{
    public class ClientRestored
    {
        public Guid ClientId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public Guid RestoredByUserId { get; set; }
        public DateTimeOffset RestoredAt { get; set; }
        public string? Reason { get; set; }
    }
}

