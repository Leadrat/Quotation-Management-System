using CRM.Application.QuotationTemplates.Dtos;

namespace CRM.Application.QuotationTemplates.Commands
{
    public class UploadQuotationTemplateCommand
    {
        public UploadQuotationTemplateRequest Request { get; set; } = null!;
        public Stream FileStream { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}

