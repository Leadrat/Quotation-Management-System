using System;

namespace CRM.Application.Auth.Commands
{
    public class InitiatePasswordResetCommand
    {
        public Guid TargetUserId { get; set; }
        public Guid AdminUserId { get; set; }
    }
}
