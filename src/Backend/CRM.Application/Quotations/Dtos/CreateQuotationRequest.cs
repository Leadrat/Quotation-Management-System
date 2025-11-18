using System;
using System.Collections.Generic;

namespace CRM.Application.Quotations.Dtos
{
    public class CreateQuotationRequest
    {
        public Guid ClientId { get; set; }
        public DateTime? QuotationDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string? Notes { get; set; }
        public List<CreateLineItemRequest> LineItems { get; set; } = new();
    }
}

