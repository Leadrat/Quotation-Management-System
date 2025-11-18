using System;

namespace CRM.Application.Quotations.Dtos
{
    public class QuotationAccessLinkDto
    {
        public Guid AccessLinkId { get; set; }
        public Guid QuotationId { get; set; }
        public string ClientEmail { get; set; } = string.Empty;
        public string ViewUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public DateTimeOffset? SentAt { get; set; }
        public DateTimeOffset? FirstViewedAt { get; set; }
        public DateTimeOffset? LastViewedAt { get; set; }
        public int ViewCount { get; set; }
        public string? IpAddress { get; set; }
    }
}
