using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Reports.Dtos
{
    public class ExportReportRequest
    {
        [Required]
        public string ReportId { get; set; } = string.Empty;

        [Required]
        public string Format { get; set; } = string.Empty; // "pdf", "excel", "csv"
    }
}

