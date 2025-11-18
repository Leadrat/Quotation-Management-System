using System.Collections.Generic;

namespace CRM.Application.Reports.Queries
{
    public class GenerateCustomReportQuery
    {
        public string ReportType { get; set; } = string.Empty;
        public Dictionary<string, object>? Filters { get; set; }
        public string? GroupBy { get; set; }
        public string? SortBy { get; set; }
        public int? Limit { get; set; }
    }
}

