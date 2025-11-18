using System;

namespace CRM.Application.Reports.Dtos
{
    public class SalesDashboardMetricsDto
    {
        public int QuotationsCreatedThisMonth { get; set; }
        public decimal TotalPipelineValue { get; set; }
        public decimal ConversionRate { get; set; }
        public int PendingApprovals { get; set; }
        public int QuotationsSentThisMonth { get; set; }
        public int QuotationsAcceptedThisMonth { get; set; }
        public List<QuotationTrendData> QuotationTrend { get; set; } = new();
        public List<StatusBreakdownData> StatusBreakdown { get; set; } = new();
        public List<TopClientData> TopClients { get; set; } = new();
        public List<RecentQuotationData> RecentQuotations { get; set; } = new();
    }

    public class QuotationTrendData
    {
        public DateTime Date { get; set; }
        public int Created { get; set; }
        public int Sent { get; set; }
    }

    public class StatusBreakdownData
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TopClientData
    {
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public decimal TotalValue { get; set; }
        public int QuotationCount { get; set; }
    }

    public class RecentQuotationData
    {
        public Guid QuotationId { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}

