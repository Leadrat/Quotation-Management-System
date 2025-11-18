namespace CRM.Application.QuotationTemplates.Queries
{
    public class GetTemplateVersionsQuery
    {
        public Guid TemplateId { get; set; }
        public Guid RequestorUserId { get; set; }
    }
}

