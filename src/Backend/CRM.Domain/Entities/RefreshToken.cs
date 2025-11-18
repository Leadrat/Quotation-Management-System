using System;

namespace CRM.Domain.Entities;

public class RefreshToken
{
    public Guid RefreshTokenId { get; set; }
    public Guid UserId { get; set; }
    public string TokenJti { get; set; } = string.Empty;
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }

    public virtual User? User { get; set; }
}
