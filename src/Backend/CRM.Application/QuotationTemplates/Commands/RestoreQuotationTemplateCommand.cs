namespace CRM.Application.QuotationTemplates.Commands
{
    public class RestoreQuotationTemplateCommand
    {
        public Guid TemplateId { get; set; }
        public Guid RestoredByUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

