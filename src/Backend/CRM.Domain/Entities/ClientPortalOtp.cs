using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    [Table("ClientPortalOtps")]
    public class ClientPortalOtp
    {
        public Guid OtpId { get; set; }
        public Guid AccessLinkId { get; set; }
        public string ClientEmail { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty; // Hashed OTP
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? VerifiedAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public int Attempts { get; set; } = 0;
        public string? IpAddress { get; set; }

        // Navigation property
        public virtual QuotationAccessLink AccessLink { get; set; } = null!;

        public bool IsExpired() => ExpiresAt < DateTimeOffset.UtcNow;
        public bool IsValid() => !IsUsed && !IsExpired() && Attempts < 5;
    }
}

