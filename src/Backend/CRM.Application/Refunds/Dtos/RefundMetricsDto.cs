using System.Collections.Generic;

namespace CRM.Application.Refunds.Dtos
{
    public class RefundMetricsDto
    {
        public decimal TotalRefundAmount { get; set; }
        public int TotalRefundCount { get; set; }
        public int PendingRefundCount { get; set; }
        public decimal RefundPercentage { get; set; }
        public decimal AverageRefundAmount { get; set; }
        public decimal AverageTAT { get; set; } // Time to approval in hours
        public List<RefundReasonBreakdown> ReasonBreakdown { get; set; } = new();
        public List<RefundStatusBreakdown> StatusBreakdown { get; set; } = new();
        public List<RefundTATByPeriod> TATByPeriod { get; set; } = new();
    }

    public class RefundReasonBreakdown
    {
        public string ReasonCode { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class RefundStatusBreakdown
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class RefundTATByPeriod
    {
        public string Period { get; set; } = string.Empty;
        public decimal AverageHours { get; set; }
    }
}

