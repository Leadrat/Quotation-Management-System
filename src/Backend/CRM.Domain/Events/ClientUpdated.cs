using System;

namespace CRM.Domain.Events
{
    public class ClientUpdated
    {
        public Guid ClientId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid UpdatedByUserId { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
