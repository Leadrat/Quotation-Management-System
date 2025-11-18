using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Reports.Dtos
{
    public class ReportGenerationRequest
    {
        [Required]
        public string ReportType { get; set; } = string.Empty;

        public Dictionary<string, object>? Filters { get; set; }

        public string? GroupBy { get; set; }

        public string? SortBy { get; set; }

        public int? Limit { get; set; }

        [Required]
        public string Format { get; set; } = "view"; // "view", "pdf", "excel", "csv"
    }
}

