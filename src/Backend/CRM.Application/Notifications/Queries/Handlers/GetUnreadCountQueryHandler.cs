using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Notifications.Dtos;
using CRM.Application.Notifications.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Queries.Handlers
{
    public class GetUnreadCountQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<GetUnreadCountQueryHandler> _logger;

        public GetUnreadCountQueryHandler(
            IAppDbContext db,
            ILogger<GetUnreadCountQueryHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<UnreadCountDto> Handle(GetUnreadCountQuery query)
        {
            try
            {
                var count = await _db.Notifications
                    .Where(n => n.RecipientUserId == query.RequestorUserId &&
                               !n.IsRead &&
                               !n.IsArchived)
                    .CountAsync();

                _logger.LogInformation("Unread count for user {UserId}: {Count}", query.RequestorUserId, count);

                return new UnreadCountDto
                {
                    Count = count
                };
            }
            catch (Exception ex) when (ex.Message.Contains("42P01") || ex.Message.Contains("does not exist") || ex.Message.Contains("relation") && ex.Message.Contains("not exist"))
            {
                _logger.LogWarning("Notifications table does not exist, returning 0 for user {UserId}", query.RequestorUserId);
                return new UnreadCountDto
                {
                    Count = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", query.RequestorUserId);
                return new UnreadCountDto
                {
                    Count = 0
                };
            }
        }
    }
}

