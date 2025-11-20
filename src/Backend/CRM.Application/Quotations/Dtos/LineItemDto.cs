using System;
using CRM.Domain.Enums;

namespace CRM.Application.Quotations.Dtos
{
    public class LineItemDto
    {
        public Guid LineItemId { get; set; }
        public int SequenceNumber { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitRate { get; set; }
        public decimal Amount { get; set; }
        
        // Product catalog integration
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public BillingCycle? BillingCycle { get; set; }
        public decimal? Hours { get; set; }
        public decimal? OriginalProductPrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public Guid? TaxCategoryId { get; set; }
    }
}

