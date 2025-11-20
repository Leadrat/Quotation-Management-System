using System;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities
{
    [Table("Products")]
    public class Product
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public ProductType ProductType { get; set; }
        public string? Description { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal? BasePricePerUserPerMonth { get; set; }
        public string? BillingCycleMultipliers { get; set; } // JSONB: {"quarterly": 0.95, "halfYearly": 0.90, "yearly": 0.85, "multiYear": 0.80}
        public string? AddOnPricing { get; set; } // JSONB: {"pricingType": "subscription|oneTime", "monthlyPrice": 50.00, "fixedPrice": 500.00}
        public string? CustomDevelopmentPricing { get; set; } // JSONB: {"pricingModel": "hourly|fixed|projectBased", "hourlyRate": 100.00, "fixedPrice": 5000.00, "baseProjectPrice": 20000.00, "estimatedHours": 200}
        public string Currency { get; set; } = "USD";
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        // Navigation properties
        public virtual ProductCategory? Category { get; set; }
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
    }
}

