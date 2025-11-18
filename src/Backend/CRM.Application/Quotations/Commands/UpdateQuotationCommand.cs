using CRM.Application.Quotations.Dtos;

namespace CRM.Application.Quotations.Commands
{
    public class UpdateQuotationCommand
    {
        public Guid QuotationId { get; set; }
        public UpdateQuotationRequest Request { get; set; } = null!;
        public Guid UpdatedByUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty;
    }
}

