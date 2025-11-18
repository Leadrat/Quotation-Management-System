using System;

namespace CRM.Domain.Events
{
    public class QuotationCreated
    {
        public Guid QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}

