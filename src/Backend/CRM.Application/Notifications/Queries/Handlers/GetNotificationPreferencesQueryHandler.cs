using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Dtos;
using CRM.Application.Notifications.Queries;
using CRM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Queries.Handlers
{
    public class GetNotificationPreferencesQueryHandler : IRequestHandler<GetNotificationPreferencesQuery, NotificationPreferencesDto>
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetNotificationPreferencesQueryHandler> _logger;

        public GetNotificationPreferencesQueryHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<GetNotificationPreferencesQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NotificationPreferencesDto> Handle(GetNotificationPreferencesQuery query, CancellationToken cancellationToken)
        {
            var preference = await _db.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == query.RequestorUserId, cancellationToken);

            if (preference == null)
            {
                // Return default preferences
                _logger.LogInformation("No preferences found for user {UserId}, returning defaults", query.RequestorUserId);
                return new NotificationPreferencesDto
                {
                    UserId = query.RequestorUserId,
                    Preferences = new Dictionary<string, Dictionary<string, bool>>()
                };
            }

            return _mapper.Map<NotificationPreferencesDto>(preference);
        }
    }
}

