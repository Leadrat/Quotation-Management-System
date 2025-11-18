using System;
using System.Collections.Generic;

namespace CRM.Application.Reports.Dtos
{
    public class ApprovalMetricsDto
    {
        public decimal AverageApprovalTAT { get; set; } // Time to approval in hours
        public decimal RejectionRate { get; set; }
        public decimal EscalationPercent { get; set; }
        public int PendingApprovals { get; set; }
        public List<ApprovalTATData> ApprovalTATByPeriod { get; set; } = new();
        public List<ApprovalStatusData> ApprovalStatusBreakdown { get; set; } = new();
        public List<EscalationData> Escalations { get; set; } = new();
    }

    public class ApprovalTATData
    {
        public string Period { get; set; } = string.Empty;
        public decimal AverageHours { get; set; }
    }

    public class ApprovalStatusData
    {
        public string Status { get; set; } = string.Empty; // "Approved", "Rejected", "Pending"
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class EscalationData
    {
        public Guid ApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public DateTimeOffset EscalatedAt { get; set; }
    }
}

