using System;

namespace CRM.Domain.Events
{
    public class ClientCreated
    {
        public Guid ClientId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid CreatedByUserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
