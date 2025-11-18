using System;

namespace CRM.Application.Users.Commands
{
    public class UpdateUserProfileCommand
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneCode { get; set; }
        public string? Mobile { get; set; }
        public Guid ActorUserId { get; set; }
        public bool IsAdminActor { get; set; }
    }
}
