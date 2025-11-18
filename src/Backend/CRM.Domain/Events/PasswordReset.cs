using System;

namespace CRM.Domain.Events
{
    public class PasswordReset
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public Guid ResetByAdminId { get; init; }
        public DateTimeOffset ResetAt { get; init; }
        public DateTimeOffset TemporaryPasswordExpiry { get; init; }
    }
}
