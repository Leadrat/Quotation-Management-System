using System;
using CRM.Domain.Enums;

namespace CRM.Application.Quotations.Dtos
{
    public class CreateLineItemRequest
    {
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitRate { get; set; }
        
        // Product catalog integration
        public Guid? ProductId { get; set; }
        public BillingCycle? BillingCycle { get; set; }
        public decimal? Hours { get; set; }
        public Guid? TaxCategoryId { get; set; }
    }
}

