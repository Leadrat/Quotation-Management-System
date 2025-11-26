using Microsoft.AspNetCore.Http;

namespace CRM.Application.DocumentTemplates.Dtos
{
    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; } = null!;
        public DocumentTemplateType TemplateType { get; set; } = DocumentTemplateType.Quotation;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public enum DocumentTemplateType
    {
        Quotation,
        ProFormaInvoice
    }
}

