using System;

namespace CRM.Domain.Events
{
    public class ClientDeleted
    {
        public Guid ClientId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public Guid DeletedByUserId { get; set; }
        public DateTimeOffset DeletedAt { get; set; }
    }
}
