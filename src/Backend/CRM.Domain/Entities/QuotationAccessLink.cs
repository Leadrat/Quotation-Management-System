using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("QuotationAccessLinks")]
    public class QuotationAccessLink
    {
        public Guid AccessLinkId { get; set; }
        public Guid QuotationId { get; set; }
        public string ClientEmail { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public DateTimeOffset? SentAt { get; set; }
        public DateTimeOffset? FirstViewedAt { get; set; }
        public DateTimeOffset? LastViewedAt { get; set; }
        public int ViewCount { get; set; } = 0;
        public string? IpAddress { get; set; }

        // Navigation properties
        public virtual Quotation Quotation { get; set; } = null!;

        public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
        public bool IsValid() => IsActive && !IsExpired();
    }
}

