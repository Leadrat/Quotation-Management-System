using System;
using CRM.Domain.Enums;

namespace CRM.Application.Products.Commands
{
    public class UpdateProductCommand
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public ProductType ProductType { get; set; }
        public string? Description { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal? BasePricePerUserPerMonth { get; set; }
        public string? BillingCycleMultipliers { get; set; }
        public string? AddOnPricing { get; set; }
        public string? CustomDevelopmentPricing { get; set; }
        public string Currency { get; set; } = "USD";
        public bool IsActive { get; set; } = true;
        public Guid UpdatedByUserId { get; set; }
    }
}

