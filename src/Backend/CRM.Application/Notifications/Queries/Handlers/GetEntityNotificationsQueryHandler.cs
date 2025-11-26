using CRM.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Dtos;
using CRM.Application.Notifications.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Queries.Handlers
{
    public class GetEntityNotificationsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetEntityNotificationsQueryHandler> _logger;

        public GetEntityNotificationsQueryHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<GetEntityNotificationsQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NotificationDto[]> Handle(GetEntityNotificationsQuery query)
        {
            var notifications = await _db.Notifications
                .AsNoTracking()
                .Where(n => n.RecipientUserId == query.RequestorUserId &&
                           n.RelatedEntityType == query.EntityType &&
                           n.RelatedEntityId == query.EntityId)
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt)
                .ToArrayAsync();

            _logger.LogInformation("Found {Count} notifications for entity {EntityType}/{EntityId} for user {UserId}",
                notifications.Length, query.EntityType, query.EntityId, query.RequestorUserId);

            return _mapper.Map<NotificationDto[]>(notifications);
        }
    }
}

