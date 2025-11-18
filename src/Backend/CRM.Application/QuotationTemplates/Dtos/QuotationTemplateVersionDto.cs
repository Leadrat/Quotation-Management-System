using System;

namespace CRM.Application.QuotationTemplates.Dtos
{
    public class QuotationTemplateVersionDto
    {
        public Guid TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Version { get; set; }
        public Guid? PreviousVersionId { get; set; }
        public string UpdatedByUserName { get; set; } = string.Empty;
        public DateTimeOffset UpdatedAt { get; set; }
        public bool IsCurrentVersion { get; set; }
    }
}

