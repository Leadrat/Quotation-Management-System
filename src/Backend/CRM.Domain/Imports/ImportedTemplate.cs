namespace CRM.Domain.Imports;

public class ImportedTemplate
{
    public Guid ImportedTemplateId { get; set; }
    public Guid ImportSessionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "generic"; // quotation, invoice, generic
    public string ContentRef { get; set; } = string.Empty; // path/blob to generated docx
    public int Version { get; set; } = 1;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
