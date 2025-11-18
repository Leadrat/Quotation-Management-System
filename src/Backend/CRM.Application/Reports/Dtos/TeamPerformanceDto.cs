using System;
using System.Collections.Generic;

namespace CRM.Application.Reports.Dtos
{
    public class TeamPerformanceDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int QuotationsCreated { get; set; }
        public int QuotationsSent { get; set; }
        public int QuotationsAccepted { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal PipelineValue { get; set; }
        public decimal AverageDiscount { get; set; }
        public int PendingApprovals { get; set; }
        public int Rank { get; set; }
        public List<PerformanceTrendData> Trend { get; set; } = new();
    }

    public class PerformanceTrendData
    {
        public DateTime Date { get; set; }
        public int QuotationsCreated { get; set; }
        public decimal ConversionRate { get; set; }
    }
}

