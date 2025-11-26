namespace CRM.Application.DocumentTemplates.Dtos
{
    public class DocumentTemplateDto
    {
        public Guid TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TemplateType { get; set; }
        public bool IsFileBased { get; set; }
        public string? TemplateFilePath { get; set; }
        public string? OriginalFileName { get; set; }
        public long? FileSizeBytes { get; set; }
        public string? ProcessingStatus { get; set; }
        public string? ProcessingErrorMessage { get; set; }
        public Guid OwnerUserId { get; set; }
        public string? OwnerUserName { get; set; }
        public string Visibility { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public int Version { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

