using System;

namespace CRM.Domain.Events
{
    public class QuotationSent
    {
        public Guid QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public Guid AccessLinkId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public DateTimeOffset SentAt { get; set; }
    }
}

 