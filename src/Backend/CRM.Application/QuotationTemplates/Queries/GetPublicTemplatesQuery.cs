namespace CRM.Application.QuotationTemplates.Queries
{
    public class GetPublicTemplatesQuery
    {
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

