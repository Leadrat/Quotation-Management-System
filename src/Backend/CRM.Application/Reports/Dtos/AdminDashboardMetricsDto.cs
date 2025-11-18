using System.Collections.Generic;

namespace CRM.Application.Reports.Dtos
{
    public class AdminDashboardMetricsDto
    {
        public int ActiveUsers { get; set; }
        public int ActiveSalesReps { get; set; }
        public int ActiveManagers { get; set; }
        public int TotalQuotationsLifetime { get; set; }
        public decimal TotalRevenue { get; set; }
        public SystemHealthData SystemHealth { get; set; } = new();
        public List<GrowthData> GrowthChart { get; set; } = new();
        public List<UsageData> UsageChart { get; set; } = new();
    }

    public class SystemHealthData
    {
        public int ErrorCount { get; set; }
        public decimal ApiUptime { get; set; }
        public decimal DatabaseSizeMB { get; set; }
        public decimal AverageResponseTimeMs { get; set; }
    }

    public class GrowthData
    {
        public string Period { get; set; } = string.Empty; // "2024-01", "2024-02", etc.
        public int Quotations { get; set; }
        public decimal Revenue { get; set; }
    }

    public class UsageData
    {
        public string Date { get; set; } = string.Empty;
        public int DailyActiveUsers { get; set; }
    }
}

