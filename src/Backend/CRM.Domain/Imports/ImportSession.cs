namespace CRM.Domain.Imports;

public class ImportSession
{
    public Guid ImportSessionId { get; set; }
    public string SourceType { get; set; } = string.Empty; // pdf, docx, xlsx, xslt, dotx
    public string SourceFileRef { get; set; } = string.Empty; // storage path or blob key
    public string Status { get; set; } = "Uploaded"; // Uploaded, Parsed, Mapped, Generated, Saved
    public string? SuggestedMappingsJson { get; set; }
    public string? ConfirmedMappingsJson { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
