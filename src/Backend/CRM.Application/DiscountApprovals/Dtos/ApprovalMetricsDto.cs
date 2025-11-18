using System;

namespace CRM.Application.DiscountApprovals.Dtos
{
    public class ApprovalMetricsDto
    {
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalCount => PendingCount + ApprovedCount + RejectedCount;
        public TimeSpan? AverageApprovalTime { get; set; }
        public decimal RejectionRate { get; set; } // Percentage
        public decimal AverageDiscountPercentage { get; set; }
        public int EscalationCount { get; set; }
        public DateTimeOffset? DateFrom { get; set; }
        public DateTimeOffset? DateTo { get; set; }
    }
}

