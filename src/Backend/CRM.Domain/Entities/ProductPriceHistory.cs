using System;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    [Table("ProductPriceHistory")]
    public class ProductPriceHistory
    {
        public Guid PriceHistoryId { get; set; }
        public Guid ProductId { get; set; }
        public PriceType PriceType { get; set; }
        public decimal? OldPriceValue { get; set; }
        public decimal NewPriceValue { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public Guid ChangedByUserId { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public string? ChangeReason { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual User ChangedByUser { get; set; } = null!;
    }
}

