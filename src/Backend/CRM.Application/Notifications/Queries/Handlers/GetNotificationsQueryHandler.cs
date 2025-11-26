using CRM.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Application.Notifications.Dtos;
using CRM.Application.Notifications.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Queries.Handlers
{
    public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetNotificationsQueryHandler> _logger;

        public GetNotificationsQueryHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<GetNotificationsQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting notifications for user {UserId}", query.RequestorUserId);

                // Base query - filter by recipient user
                IQueryable<UserNotification> baseQuery = _db.Notifications
                    .AsNoTracking()
                    .Where(n => n.RecipientUserId == query.RequestorUserId)
                    .Include(n => n.User);

                // Apply filters
                if (query.Unread.HasValue)
                {
                    baseQuery = baseQuery.Where(n => n.IsRead == !query.Unread.Value);
                }

                if (query.Archived.HasValue)
                {
                    baseQuery = baseQuery.Where(n => n.IsArchived == query.Archived.Value);
                }

                if (!string.IsNullOrEmpty(query.EventType))
                {
                    baseQuery = baseQuery.Where(n => n.EventType == query.EventType);
                }

                if (!string.IsNullOrEmpty(query.EntityType))
                {
                    baseQuery = baseQuery.Where(n => n.RelatedEntityType == query.EntityType);
                }

                if (query.EntityId.HasValue)
                {
                    baseQuery = baseQuery.Where(n => n.RelatedEntityId == query.EntityId.Value);
                }

                if (query.DateFrom.HasValue)
                {
                    baseQuery = baseQuery.Where(n => n.CreatedAt >= query.DateFrom.Value);
                }

                if (query.DateTo.HasValue)
                {
                    baseQuery = baseQuery.Where(n => n.CreatedAt <= query.DateTo.Value);
                }

                // Get total count before pagination
                var totalCount = await baseQuery.CountAsync(cancellationToken);

                // Apply pagination and ordering
                var notifications = await baseQuery
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync(cancellationToken);

                var dtos = _mapper.Map<NotificationDto[]>(notifications);

                return new PagedResult<NotificationDto>
                {
                    Success = true,
                    Data = dtos,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex) when (ex.Message.Contains("42P01") || ex.Message.Contains("does not exist") || ex.Message.Contains("relation") && ex.Message.Contains("not exist"))
            {
                _logger.LogWarning("Notifications table does not exist, returning empty result for user {UserId}", query.RequestorUserId);
                return new PagedResult<NotificationDto>
                {
                    Success = true,
                    Data = Array.Empty<NotificationDto>(),
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", query.RequestorUserId);
                return new PagedResult<NotificationDto>
                {
                    Success = true,
                    Data = Array.Empty<NotificationDto>(),
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = 0
                };
            }
        }
    }
}

