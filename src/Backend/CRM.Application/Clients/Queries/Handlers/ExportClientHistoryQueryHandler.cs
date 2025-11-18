using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.Clients.Dtos;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Queries.Handlers
{
    public class ExportClientHistoryQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public ExportClientHistoryQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ClientHistoryEntryDto>> Handle(ExportClientHistoryQuery request)
        {
            var isAdmin = string.Equals(request.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            var isManager = string.Equals(request.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && !isManager)
            {
                throw new UnauthorizedAccessException("Only Admin and Manager roles can export history.");
            }

            var maxRows = request.MaxRows <= 0 ? 5000 : Math.Min(request.MaxRows, 5000);

            var query = _db.ClientHistories.AsNoTracking();

            // Filter by client IDs if provided
            if (request.ClientIds != null && request.ClientIds.Any())
            {
                query = query.Where(h => request.ClientIds.Contains(h.ClientId));
            }

            // Apply authorization: Admins can see all; Managers can see their team's clients
            if (!isAdmin && isManager)
            {
                // For managers, filter to clients created by their direct reports
                // This is a simplified check - in production, you might need a more complex query
                var teamUserIds = await _db.Users
                    .Where(u => u.ReportingManagerId == request.RequestorUserId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                query = query.Where(h => h.Client != null && teamUserIds.Contains(h.Client.CreatedByUserId));
            }

            if (!string.IsNullOrWhiteSpace(request.ActionType))
            {
                query = query.Where(h => h.ActionType == request.ActionType);
            }

            if (request.DateFrom.HasValue)
            {
                query = query.Where(h => h.CreatedAt >= request.DateFrom.Value);
            }

            if (request.DateTo.HasValue)
            {
                query = query.Where(h => h.CreatedAt <= request.DateTo.Value);
            }

            var data = await query
                .OrderByDescending(h => h.CreatedAt)
                .ThenByDescending(h => h.HistoryId)
                .Take(maxRows)
                .Include(h => h.ActorUser)
                .Include(h => h.Client)
                .ProjectTo<ClientHistoryEntryDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return data;
        }
    }
}

