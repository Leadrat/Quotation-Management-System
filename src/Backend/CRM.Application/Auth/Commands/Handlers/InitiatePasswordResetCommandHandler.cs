using System;
using System.Threading.Tasks;
using CRM.Application.Auth.Commands;
using CRM.Application.Common.Notifications;
using CRM.Application.Common.Security;
using CRM.Domain.Entities;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Auth.Commands.Handlers
{
    public class InitiatePasswordResetCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IResetTokenGenerator _tokenGen;
        private readonly IEmailQueue _emails;

        public InitiatePasswordResetCommandHandler(IAppDbContext db, IResetTokenGenerator tokenGen, IEmailQueue emails)
        {
            _db = db;
            _tokenGen = tokenGen;
            _emails = emails;
        }

        public async Task Handle(InitiatePasswordResetCommand cmd)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == cmd.TargetUserId);
            if (user == null) throw new InvalidOperationException("User not found");

            // Revoke previous active tokens for this user
            var prev = await _db.PasswordResetTokens
                .Where(t => t.UserId == user.UserId && t.UsedAt == null)
                .ToListAsync();
            foreach (var t in prev)
            {
                t.UsedAt = DateTimeOffset.UtcNow;
            }

            var (token, hash) = _tokenGen.Generate();
            var entity = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.UserId,
                TokenHash = hash,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.PasswordResetTokens.Add(entity);
            await _db.SaveChangesAsync();

            var link = $"https://app.crm.com/reset-password?token={token}";
            await _emails.EnqueueAsync(new EmailMessage(
                user.Email,
                "Password reset request",
                $"<p>Hi {user.FirstName}, an admin initiated a password reset for your account. Use this one-time link within 24 hours:</p><p><a href='{link}'>Reset Password</a></p>"
            ));
        }
    }
}
