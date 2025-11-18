using System;

namespace CRM.Application.Quotations.Dtos
{
    public class QuotationStatusHistoryDto
    {
        public Guid HistoryId { get; set; }
        public Guid QuotationId { get; set; }
        public string? PreviousStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? ChangedByUserName { get; set; }
        public string? Reason { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public string? IpAddress { get; set; }
    }
}

