using System.Collections.Generic;

namespace CRM.Application.Quotations.Dtos
{
    public class SendQuotationRequest
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public List<string>? CcEmails { get; set; }
        public List<string>? BccEmails { get; set; }
        public string? CustomMessage { get; set; }
    }
}
