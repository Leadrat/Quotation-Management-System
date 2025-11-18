using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("QuotationStatusHistory")]
    public class QuotationStatusHistory
    {
        public Guid HistoryId { get; set; }
        public Guid QuotationId { get; set; }
        public string? PreviousStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public Guid? ChangedByUserId { get; set; }
        public string? Reason { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public string? IpAddress { get; set; }

        // Navigation properties
        public virtual Quotation Quotation { get; set; } = null!;
        public virtual User? ChangedByUser { get; set; }
    }
}

