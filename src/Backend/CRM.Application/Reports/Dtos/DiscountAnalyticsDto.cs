using System;
using System.Collections.Generic;

namespace CRM.Application.Reports.Dtos
{
    public class DiscountAnalyticsDto
    {
        public decimal AverageDiscountPercent { get; set; }
        public decimal ApprovalRate { get; set; }
        public decimal MarginImpact { get; set; }
        public List<DiscountByRepData> DiscountByRep { get; set; } = new();
        public List<ApprovalRateData> ApprovalRates { get; set; } = new();
        public List<MarginImpactData> MarginImpactByPeriod { get; set; } = new();
    }

    public class DiscountByRepData
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal AverageDiscount { get; set; }
        public int RequestCount { get; set; }
    }

    public class ApprovalRateData
    {
        public string Status { get; set; } = string.Empty; // "Approved", "Rejected", "Pending"
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class MarginImpactData
    {
        public string Period { get; set; } = string.Empty;
        public decimal TotalDiscountAmount { get; set; }
        public decimal MarginImpact { get; set; }
    }
}

