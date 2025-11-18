namespace CRM.Application.QuotationTemplates.Commands
{
    public class ApplyTemplateToQuotationCommand
    {
        public Guid TemplateId { get; set; }
        public Guid ClientId { get; set; }
        public Guid AppliedByUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

