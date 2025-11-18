using System;

namespace CRM.Domain.Entities
{
    public class PasswordResetToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public byte[] TokenHash { get; set; } = Array.Empty<byte>();
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? UsedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
