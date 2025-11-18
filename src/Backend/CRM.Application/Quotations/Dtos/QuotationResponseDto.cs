using System;

namespace CRM.Application.Quotations.Dtos
{
    public class QuotationResponseDto
    {
        public Guid ResponseId { get; set; }
        public Guid QuotationId { get; set; }
        public string ResponseType { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public string? ResponseMessage { get; set; }
        public DateTimeOffset ResponseDate { get; set; }
        public string? IpAddress { get; set; }
        public DateTimeOffset? NotifiedAdminAt { get; set; }
    }
}
