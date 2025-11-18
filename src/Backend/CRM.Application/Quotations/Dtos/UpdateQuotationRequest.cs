using System;
using System.Collections.Generic;

namespace CRM.Application.Quotations.Dtos
{
    public class UpdateQuotationRequest
    {
        public DateTime? QuotationDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string? Notes { get; set; }
        public List<UpdateLineItemRequest>? LineItems { get; set; }
    }
}

