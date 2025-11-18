namespace CRM.Application.Quotations.Queries
{
    public class GetQuotationByIdQuery
    {
        public Guid QuotationId { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

