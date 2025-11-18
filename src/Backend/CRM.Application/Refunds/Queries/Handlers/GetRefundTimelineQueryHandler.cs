using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Refunds.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Refunds.Queries.Handlers
{
    public class GetRefundTimelineQueryHandler
    {
        private readonly IAppDbContext _db;

        public GetRefundTimelineQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<RefundTimelineDto>> Handle(GetRefundTimelineQuery query)
        {
            var timeline = await _db.RefundTimeline
                .Include(t => t.ActedByUser)
                .Where(t => t.RefundId == query.RefundId)
                .OrderBy(t => t.EventDate)
                .ToListAsync();

            return timeline.Select(t => new RefundTimelineDto
            {
                TimelineId = t.TimelineId,
                RefundId = t.RefundId,
                EventType = t.EventType,
                ActedByUserId = t.ActedByUserId,
                ActedByUserName = t.ActedByUser != null 
                    ? $"{t.ActedByUser.FirstName} {t.ActedByUser.LastName}" 
                    : string.Empty,
                Comments = t.Comments,
                EventDate = t.EventDate,
                IpAddress = t.IpAddress
            }).ToList();
        }
    }
}

