using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("ProductServiceCategories")]
    public class ProductServiceCategory
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryCode { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        // Navigation properties
        // public virtual ICollection<TaxRate> TaxRates { get; set; } = new List<TaxRate>(); // Commented out until TaxRate entity is created
        // public virtual ICollection<QuotationLineItem> QuotationLineItems { get; set; } = new List<QuotationLineItem>(); // Commented out until QuotationLineItem entity is updated

        // Domain methods
        public bool IsDeleted() => DeletedAt != null;
    }
}

