namespace CRM.Application.QuotationTemplates.Queries
{
    public class GetTemplateByIdQuery
    {
        public Guid TemplateId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

