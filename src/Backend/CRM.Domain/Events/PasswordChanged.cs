using System;

namespace CRM.Domain.Events
{
    public class PasswordChanged
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public DateTimeOffset ChangedAt { get; init; }
        public Guid ChangedByUserId { get; init; }
        public string? IPAddress { get; init; }
        public string? UserAgent { get; init; }
    }
}
