using System;
using System.ComponentModel.DataAnnotations.Schema;

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
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation property
        public virtual Quotation Quotation { get; set; } = null!;

        // Domain method
        public void CalculateAmount()
        {
            Amount = Quantity * UnitRate;
        }
    }
}

