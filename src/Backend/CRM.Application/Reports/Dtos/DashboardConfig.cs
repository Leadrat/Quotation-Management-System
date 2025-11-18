using System.Collections.Generic;

namespace CRM.Application.Reports.Dtos
{
    public class DashboardConfig
    {
        public string Layout { get; set; } = "grid"; // "grid", "flex"
        public List<DashboardWidget> Widgets { get; set; } = new();
        public Dictionary<string, object>? Filters { get; set; }
    }

    public class DashboardWidget
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "kpi", "lineChart", "barChart", "pieChart", etc.
        public string Metric { get; set; } = string.Empty;
        public WidgetPosition Position { get; set; } = new();
    }

    public class WidgetPosition
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}

