using System;

namespace CRM.Domain.Events
{
    public class QuotationViewed
    {
        public Guid QuotationId { get; set; }
        public Guid AccessLinkId { get; set; }
        public int ViewCount { get; set; }
        public DateTimeOffset ViewedAt { get; set; }
        public string? IpAddress { get; set; }
    }
}

 