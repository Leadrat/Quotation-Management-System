using System;

namespace CRM.Application.Reports.Queries
{
    public class GetForecastingDataQuery
    {
        public int Days { get; set; } = 30;
        public decimal ConfidenceLevel { get; set; } = 0.95m;
    }
}

