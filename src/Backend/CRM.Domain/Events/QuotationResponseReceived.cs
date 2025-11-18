using System;

namespace CRM.Domain.Events
{
    public class QuotationResponseReceived
    {
        public Guid QuotationId { get; set; }
        public Guid ResponseId { get; set; }
        public string ResponseType { get; set; } = string.Empty;
        public DateTimeOffset ResponseDate { get; set; }
        public string ClientEmail { get; set; } = string.Empty;
    }
}

 