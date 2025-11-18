using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Reports.Dtos
{
    public class ScheduleReportRequest
    {
        [Required]
        [MaxLength(200)]
        public string ReportName { get; set; } = string.Empty;

        [Required]
        public string ReportType { get; set; } = string.Empty;

        [Required]
        public Dictionary<string, object> ReportConfig { get; set; } = new();

        [Required]
        public string RecurrencePattern { get; set; } = string.Empty; // "daily", "weekly", "monthly"

        [Required]
        public string EmailRecipients { get; set; } = string.Empty; // Comma-separated emails
    }
}

