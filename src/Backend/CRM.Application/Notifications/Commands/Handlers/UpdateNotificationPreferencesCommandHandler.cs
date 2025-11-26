using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Commands;
using CRM.Application.Notifications.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Commands.Handlers
{
    public class UpdateNotificationPreferencesCommandHandler : IRequestHandler<UpdateNotificationPreferencesCommand, NotificationPreferencesDto>
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateNotificationPreferencesCommandHandler> _logger;

        public UpdateNotificationPreferencesCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<UpdateNotificationPreferencesCommandHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NotificationPreferencesDto> Handle(UpdateNotificationPreferencesCommand command, CancellationToken cancellationToken)
        {
            var preference = await _db.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == command.UserId, cancellationToken);

            if (preference == null)
            {
                // Create new preference
                preference = new NotificationPreference
                {
                    UserId = command.UserId,
                    PreferenceData = "{}",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _db.NotificationPreferences.Add(preference);
            }

            // Update preferences
            preference.UpdatePreferences(command.Preferences);
            await _db.SaveChangesAsync(cancellationToken);

            // Publish domain event
            var evt = new UserNotificationPreferenceUpdated
            {
                UserId = command.UserId,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _logger.LogInformation("Notification preferences updated for user {UserId}", command.UserId);

            return _mapper.Map<NotificationPreferencesDto>(preference);
        }
    }
}

