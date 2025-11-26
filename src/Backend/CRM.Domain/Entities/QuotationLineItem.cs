using System;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    [Table("QuotationLineItems")]
    public class QuotationLineItem
    {
        public Guid LineItemId { get; set; }
        public Guid QuotationId { get; set; }
        public int SequenceNumber { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitRate { get; set; }
        public decimal Amount { get; set; }
        public Guid? ProductServiceCategoryId { get; set; }
        
        // Product catalog integration fields
        public Guid? ProductId { get; set; }
        public BillingCycle? BillingCycle { get; set; }
        public decimal? Hours { get; set; }
        public decimal? OriginalProductPrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public Guid? TaxCategoryId { get; set; }
        
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation properties
        public virtual Quotation Quotation { get; set; } = null!;
        public virtual Product? Product { get; set; }
        public virtual ProductServiceCategory? TaxCategory { get; set; }

        // Domain method
        public void CalculateAmount()
        {
            Amount = Quantity * UnitRate;
            if (DiscountAmount.HasValue && DiscountAmount.Value > 0)
            {
                Amount -= DiscountAmount.Value;
            }
        }
    }
}

