using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Application.Notifications.Dtos;
using CRM.Application.Notifications.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Queries.Handlers
{
    public class GetEmailNotificationLogsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetEmailNotificationLogsQueryHandler> _logger;

        public GetEmailNotificationLogsQueryHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<GetEmailNotificationLogsQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<EmailNotificationLogDto>> Handle(GetEmailNotificationLogsQuery query)
        {
            // Require Admin role
            if (!string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Only admins can access email notification logs");
            }

            _logger.LogInformation("Getting email notification logs for admin {UserId}", query.RequestorUserId);

            // Base query
            IQueryable<Domain.Entities.EmailNotificationLog> baseQuery = _db.EmailNotificationLogs
                .AsNoTracking();

            // Apply filters
            if (query.UserId.HasValue)
            {
                baseQuery = baseQuery.Where(log => 
                    log.Notification != null && 
                    log.Notification.RecipientUserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.RecipientEmail))
            {
                baseQuery = baseQuery.Where(log => 
                    log.RecipientEmail.ToLower().Contains(query.RecipientEmail.ToLower()));
            }

            if (!string.IsNullOrEmpty(query.EventType))
            {
                baseQuery = baseQuery.Where(log => log.EventType == query.EventType);
            }

            if (!string.IsNullOrEmpty(query.Status))
            {
                baseQuery = baseQuery.Where(log => log.Status == query.Status);
            }

            if (query.DateFrom.HasValue)
            {
                baseQuery = baseQuery.Where(log => log.SentAt >= query.DateFrom.Value);
            }

            if (query.DateTo.HasValue)
            {
                baseQuery = baseQuery.Where(log => log.SentAt <= query.DateTo.Value);
            }

            // Get total count before pagination
            var totalCount = await baseQuery.CountAsync();

            // Apply pagination and ordering
            var logs = await baseQuery
                .OrderByDescending(log => log.SentAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToArrayAsync();

            var dtos = _mapper.Map<EmailNotificationLogDto[]>(logs);

            return new PagedResult<EmailNotificationLogDto>
            {
                Success = true,
                Data = dtos,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }
    }
}

