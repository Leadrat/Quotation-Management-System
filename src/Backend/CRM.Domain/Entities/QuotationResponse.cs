using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("QuotationResponses")]
    public class QuotationResponse
    {
        public Guid ResponseId { get; set; }
        public Guid QuotationId { get; set; }
        public string ResponseType { get; set; } = string.Empty; // ACCEPTED, REJECTED, NEEDS_MODIFICATION
        public string ClientEmail { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public string? ResponseMessage { get; set; }
        public DateTimeOffset ResponseDate { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTimeOffset? NotifiedAdminAt { get; set; }

        // Navigation properties
        public virtual Quotation Quotation { get; set; } = null!;
    }
}

