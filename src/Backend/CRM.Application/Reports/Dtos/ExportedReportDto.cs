using System;

namespace CRM.Application.Reports.Dtos
{
    public class ExportedReportDto
    {
        public Guid ExportId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string ExportFormat { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int FileSize { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
    }
}

