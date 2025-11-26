using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("TemplatePlaceholders")]
    public class TemplatePlaceholder
    {
        public Guid PlaceholderId { get; set; }
        public Guid TemplateId { get; set; }
        public string PlaceholderName { get; set; } = string.Empty;
        public string PlaceholderType { get; set; } = string.Empty; // 'Company', 'Customer', or 'Quotation'
        public string? OriginalText { get; set; }
        public string? DefaultValue { get; set; }
        public int? PositionInDocument { get; set; }
        public bool IsManuallyAdded { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation properties
        // Navigation properties
        public virtual DocumentTemplate Template { get; set; } = null!;
    }
}

