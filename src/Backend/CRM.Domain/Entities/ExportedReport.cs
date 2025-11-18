using System;

namespace CRM.Domain.Entities
{
    /// <summary>
    /// Tracks exported report files for download history
    /// </summary>
    public class ExportedReport
    {
        public Guid ExportId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string ExportFormat { get; set; } = string.Empty; // "pdf", "excel", "csv"
        public string FilePath { get; set; } = string.Empty;
        public int FileSize { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // Navigation
        public User CreatedByUser { get; set; } = null!;
    }
}

