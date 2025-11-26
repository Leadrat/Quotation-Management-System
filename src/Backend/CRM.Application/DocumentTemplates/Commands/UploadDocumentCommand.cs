using CRM.Application.DocumentTemplates.Dtos;

namespace CRM.Application.DocumentTemplates.Commands
{
    public class UploadDocumentCommand
    {
        public UploadDocumentRequest Request { get; set; } = null!;
        public Stream FileStream { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}

