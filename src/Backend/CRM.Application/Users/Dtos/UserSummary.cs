using System;

namespace CRM.Application.Users.Dtos
{
    public class UserSummary
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public string? PhoneCode { get; set; }
    }
}
