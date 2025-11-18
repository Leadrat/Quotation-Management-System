using CRM.Application.Quotations.Dtos;

namespace CRM.Application.Quotations.Commands
{
    public class CreateQuotationCommand
    {
        public CreateQuotationRequest Request { get; set; } = null!;
        public Guid CreatedByUserId { get; set; }
    }
}

