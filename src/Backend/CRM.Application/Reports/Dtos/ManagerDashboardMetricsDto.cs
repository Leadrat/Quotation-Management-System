using System;
using System.Collections.Generic;

namespace CRM.Application.Reports.Dtos
{
    public class ManagerDashboardMetricsDto
    {
        public int TeamQuotationsThisMonth { get; set; }
        public decimal TeamConversionRate { get; set; }
        public decimal AverageDiscountPercent { get; set; }
        public int PendingApprovals { get; set; }
        public decimal TotalValueAtRisk { get; set; }
        public List<TeamQuotaData> TeamQuotaVsActual { get; set; } = new();
        public List<RepPerformanceData> RepPerformance { get; set; } = new();
        public List<PipelineStageData> PipelineStages { get; set; } = new();
        public List<DiscountComplianceData> DiscountCompliance { get; set; } = new();
        public List<TeamMemberData> TeamMembers { get; set; } = new();
        public List<PendingApprovalData> PendingApprovalsList { get; set; } = new();
    }

    public class TeamQuotaData
    {
        public string Period { get; set; } = string.Empty; // "Week 1", "Week 2", etc.
        public decimal Quota { get; set; }
        public decimal Actual { get; set; }
    }

    public class RepPerformanceData
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int QuotationsCreated { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal AverageDiscount { get; set; }
    }

    public class PipelineStageData
    {
        public string Stage { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    public class DiscountComplianceData
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal AverageDiscount { get; set; }
        public string Status { get; set; } = string.Empty; // "green", "yellow", "red"
    }

    public class TeamMemberData
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int QuotationsCreated { get; set; }
        public decimal PipelineValue { get; set; }
        public decimal ConversionRate { get; set; }
        public int PendingApprovals { get; set; }
        public string Status { get; set; } = string.Empty; // "green", "yellow", "red"
    }

    public class PendingApprovalData
    {
        public Guid ApprovalId { get; set; }
        public Guid QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTimeOffset RequestedAt { get; set; }
    }
}

