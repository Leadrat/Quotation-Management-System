using System;
using System.Collections.Generic;

namespace CRM.Application.QuotationTemplates.Dtos
{
    public class TemplateUsageStatsDto
    {
        public int TotalTemplates { get; set; }
        public int TotalUsage { get; set; }
        public int ApprovedTemplates { get; set; }
        public int PendingApprovalTemplates { get; set; }
        public List<MostUsedTemplateDto> MostUsedTemplates { get; set; } = new();
        public Dictionary<string, int> UsageByVisibility { get; set; } = new();
        public Dictionary<string, int> UsageByRole { get; set; } = new();
    }

    public class MostUsedTemplateDto
    {
        public Guid TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public DateTimeOffset? LastUsedAt { get; set; }
    }
}

