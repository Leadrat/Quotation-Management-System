using System;
using System.Collections.Generic;

namespace CRM.Application.Quotations.Dtos
{
    public class QuotationDto
    {
        public Guid QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool IsExpired { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal? CgstAmount { get; set; }
        public decimal? SgstAmount { get; set; }
        public decimal? IgstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public List<LineItemDto> LineItems { get; set; } = new();
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

