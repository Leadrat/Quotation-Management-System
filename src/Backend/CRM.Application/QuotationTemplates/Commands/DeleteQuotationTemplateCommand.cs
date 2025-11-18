namespace CRM.Application.QuotationTemplates.Commands
{
    public class DeleteQuotationTemplateCommand
    {
        public Guid TemplateId { get; set; }
        public Guid DeletedByUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

