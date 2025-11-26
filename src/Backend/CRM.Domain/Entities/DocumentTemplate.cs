using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.Entities
{
    public class DocumentTemplate
    {
        [Key]
        public Guid TemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TemplateType { get; set; } // "Quotation" or "ProformaInvoice"
        public string FilePath { get; set; } // Path to the stored .docx file
        public string OriginalFileName { get; set; }
        public long FileSizeBytes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }

        // Navigation property for placeholders if we decide to store them relationally
        // For now, we might just parse them on the fly or store as JSON, but let's define a collection
        public ICollection<TemplatePlaceholder> Placeholders { get; set; } = new List<TemplatePlaceholder>();

        public DocumentTemplate()
        {
            TemplateId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }
}
