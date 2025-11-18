using System;

namespace CRM.Domain.Entities;

public class EmailVerificationToken
{
    public Guid TokenId { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public virtual User? User { get; set; }
}
