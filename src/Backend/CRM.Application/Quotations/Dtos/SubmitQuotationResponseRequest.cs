namespace CRM.Application.Quotations.Dtos
{
    public class SubmitQuotationResponseRequest
    {
        public string ResponseType { get; set; } = string.Empty; // ACCEPTED, REJECTED, NEEDS_MODIFICATION
        public string? ClientName { get; set; }
        public string? ResponseMessage { get; set; }
        public string? ClientEmail { get; set; }
    }
}

