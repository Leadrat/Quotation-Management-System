using System.Collections.Generic;

namespace CRM.Application.Reports.Dtos
{
    public class ReportData
    {
        public string ReportType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public List<KPIMetric> Metrics { get; set; } = new();
        public List<ChartData> Charts { get; set; } = new();
        public List<Dictionary<string, object>> Details { get; set; } = new();
    }

    public class KPIMetric
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public decimal? NumericValue { get; set; }
        public string? Trend { get; set; } // "up", "down", "stable"
        public string? Color { get; set; } // "green", "red", "yellow"
    }

    public class ChartData
    {
        public string ChartType { get; set; } = string.Empty; // "line", "bar", "pie", "funnel", "heatmap"
        public string Title { get; set; } = string.Empty;
        public List<ChartSeries> Series { get; set; } = new();
        public List<string> Categories { get; set; } = new();
    }

    public class ChartSeries
    {
        public string Name { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new();
    }
}

