using CRM.Application.QuotationTemplates.Dtos;

namespace CRM.Application.QuotationTemplates.Commands
{
    public class CreateQuotationTemplateCommand
    {
        public CreateQuotationTemplateRequest Request { get; set; } = null!;
        public Guid CreatedByUserId { get; set; }
    }
}

