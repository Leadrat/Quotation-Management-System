using System;
using System.Collections.Generic;

namespace CRM.Application.Reports.Dtos
{
    public class ForecastingDataDto
    {
        public decimal PredictedRevenue { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public decimal PredictedSuccessRate { get; set; }
        public List<RevenueForecastData> RevenueForecast { get; set; } = new();
        public List<TrendData> Trend { get; set; } = new();
    }

    public class RevenueForecastData
    {
        public DateTime Date { get; set; }
        public decimal PredictedRevenue { get; set; }
        public decimal? LowerBound { get; set; }
        public decimal? UpperBound { get; set; }
    }

    public class TrendData
    {
        public DateTime Date { get; set; }
        public decimal ActualValue { get; set; }
        public decimal? PredictedValue { get; set; }
    }
}

