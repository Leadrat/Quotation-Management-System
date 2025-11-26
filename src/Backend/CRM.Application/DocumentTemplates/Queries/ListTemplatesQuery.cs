using System;

namespace CRM.Application.DocumentTemplates.Queries
{
    public class ListTemplatesQuery
    {
        public string? TemplateType { get; set; }
        public Guid RequestedByUserId { get; set; }
        public string RequestedByRole { get; set; } = string.Empty;
    }
}

