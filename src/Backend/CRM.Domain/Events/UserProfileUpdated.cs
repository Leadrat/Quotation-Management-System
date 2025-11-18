using System;

namespace CRM.Domain.Events
{
    public class UserProfileUpdated
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public DateTimeOffset UpdatedAt { get; init; }
        public Guid UpdatedByUserId { get; init; }
    }
}
