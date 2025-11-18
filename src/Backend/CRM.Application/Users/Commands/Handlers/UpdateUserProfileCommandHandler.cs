using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Users.Dtos;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using CRM.Application.Common.Notifications;
using CRM.Domain.Events;

namespace CRM.Application.Users.Commands.Handlers
{
    public class UpdateUserProfileCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IEmailQueue _emails;

        public UpdateUserProfileCommandHandler(IAppDbContext db, IMapper mapper, IEmailQueue emails)
        {
            _db = db;
            _mapper = mapper;
            _emails = emails;
        }

        public async Task<UserSummary> Handle(UpdateUserProfileCommand cmd)
        {
            var target = await _db.Users.FirstOrDefaultAsync(u => u.UserId == cmd.UserId);
            if (target == null)
                throw new InvalidOperationException("User not found");

            // Authorization: self or admin
            if (!(cmd.IsAdminActor || cmd.ActorUserId == cmd.UserId))
                throw new UnauthorizedAccessException("Not allowed to update this profile");

            // Immutable fields: Email, RoleId, ReportingManagerId
            target.FirstName = cmd.FirstName.Trim();
            target.LastName = cmd.LastName.Trim();
            target.PhoneCode = cmd.PhoneCode;
            target.Mobile = string.IsNullOrWhiteSpace(cmd.Mobile) ? null : cmd.Mobile;
            target.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Emit event (simplified publish pattern)
            var ev = new UserProfileUpdated
            {
                UserId = target.UserId,
                Email = target.Email,
                FirstName = target.FirstName,
                LastName = target.LastName,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedByUserId = cmd.ActorUserId
            };
            // In this codebase, events are typically logged/handled elsewhere; we enqueue email directly here
            await _emails.EnqueueAsync(new EmailMessage(
                target.Email,
                "Your profile was updated",
                $"<p>Hi {target.FirstName}, your profile details were updated on {DateTime.UtcNow:u}.</p>"
            ));

            return _mapper.Map<UserSummary>(target);
        }
    }
}
