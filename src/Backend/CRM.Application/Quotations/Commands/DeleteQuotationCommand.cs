namespace CRM.Application.Quotations.Commands
{
    public class DeleteQuotationCommand
    {
        public Guid QuotationId { get; set; }
        public Guid DeletedByUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

