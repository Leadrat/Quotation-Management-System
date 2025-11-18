namespace CRM.Application.QuotationTemplates.Commands
{
    public class ApproveQuotationTemplateCommand
    {
        public Guid TemplateId { get; set; }
        public Guid ApprovedByUserId { get; set; }
    }
}

