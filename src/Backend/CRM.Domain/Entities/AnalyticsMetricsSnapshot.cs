using System;
using System.Text.Json;

namespace CRM.Domain.Entities
{
    /// <summary>
    /// Caches pre-calculated metrics for performance optimization
    /// </summary>
    public class AnalyticsMetricsSnapshot
    {
        public Guid SnapshotId { get; set; }
        public string MetricType { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public JsonDocument MetricData { get; set; } = null!;
        public DateTimeOffset CalculatedAt { get; set; }
        public DateTime PeriodDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // Navigation
        public User? User { get; set; }
    }
}

