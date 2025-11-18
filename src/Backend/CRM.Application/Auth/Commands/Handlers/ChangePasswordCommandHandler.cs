using System;
using System.Threading.Tasks;
using CRM.Application.Auth.Commands;
using CRM.Application.Auth.Services;
using CRM.Application.Common.Notifications;
using CRM.Application.Common.Security;
using CRM.Application.Common.Persistence;
using CRM.Shared.Exceptions;
using CRM.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Auth.Commands.Handlers
{
    public class ChangePasswordCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly IRefreshTokenRevoker _revoker;
        private readonly IEmailQueue _emails;

        public ChangePasswordCommandHandler(IAppDbContext db, IPasswordHasher hasher, IRefreshTokenRevoker revoker, IEmailQueue emails)
        {
            _db = db;
            _hasher = hasher;
            _revoker = revoker;
            _emails = emails;
        }

        public async Task Handle(ChangePasswordCommand cmd)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == cmd.UserId);
            if (user == null) throw new InvalidOperationException("User not found");
            if (user.IsLockedOut) throw new AccountLockedException();

            // Verify current password
            var ok = _hasher.Verify(user.PasswordHash, cmd.CurrentPassword);
            if (!ok)
            {
                user.LoginAttempts += 1;
                if (user.LoginAttempts >= 5)
                {
                    user.IsLockedOut = true;
                }
                await _db.SaveChangesAsync();
                throw new InvalidCurrentPasswordException();
            }

            // Strength check (defense-in-depth beyond validator)
            if (!System.Text.RegularExpressions.Regex.IsMatch(cmd.NewPassword, PasswordPolicy.StrengthPattern))
            {
                throw new WeakPasswordException();
            }

            // Reuse check
            if (_hasher.Verify(user.PasswordHash, cmd.NewPassword))
            {
                throw new PasswordReuseException();
            }

            user.PasswordHash = _hasher.Hash(cmd.NewPassword);
            user.LoginAttempts = 0;
            user.IsLockedOut = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _revoker.RevokeAllForUserAsync(user.UserId);

            await _emails.EnqueueAsync(new EmailMessage(
                user.Email,
                "Your password was changed",
                $"<p>Hi {user.FirstName}, your password was changed on {DateTime.UtcNow:u}. If this wasn't you, contact support immediately.</p>"
            ));
        }
    }
}
