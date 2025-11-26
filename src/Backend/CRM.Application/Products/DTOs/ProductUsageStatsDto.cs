using System;

namespace CRM.Application.Products.DTOs
{
    public class ProductUsageStatsDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuotationsUsedIn { get; set; }
        public decimal TotalRevenueGenerated { get; set; }
        public string Currency { get; set; } = "USD";
    }
}

