using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("QuotationTemplateLineItems")]
    public class QuotationTemplateLineItem
    {
        public Guid LineItemId { get; set; }
        public Guid TemplateId { get; set; }
        public int SequenceNumber { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitRate { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // Navigation property
        public virtual QuotationTemplate Template { get; set; } = null!;

        // Domain method
        public void CalculateAmount()
        {
            Amount = Quantity * UnitRate;
        }
    }
}

