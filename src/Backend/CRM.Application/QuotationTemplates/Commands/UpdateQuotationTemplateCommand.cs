using CRM.Application.QuotationTemplates.Dtos;

namespace CRM.Application.QuotationTemplates.Commands
{
    public class UpdateQuotationTemplateCommand
    {
        public Guid TemplateId { get; set; }
        public UpdateQuotationTemplateRequest Request { get; set; } = null!;
        public Guid UpdatedByUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

