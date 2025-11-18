using System;

namespace CRM.Domain.Events
{
    public class QuotationDeleted
    {
        public Guid QuotationId { get; set; }
        public Guid DeletedByUserId { get; set; }
        public DateTimeOffset DeletedAt { get; set; }
    }
}

