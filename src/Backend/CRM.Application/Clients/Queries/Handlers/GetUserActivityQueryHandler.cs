using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.Clients.Dtos;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CRM.Application.Clients.Queries.Handlers
{
    public class GetUserActivityQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly HistorySettings _historySettings;

        public GetUserActivityQueryHandler(IAppDbContext db, IMapper mapper, IOptions<HistorySettings> historySettings)
        {
            _db = db;
            _mapper = mapper;
            _historySettings = historySettings.Value;
        }

        public async Task<PagedResult<ClientHistoryEntryDto>> Handle(GetUserActivityQuery request)
        {
            var isAdmin = string.Equals(request.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            var isManager = string.Equals(request.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase);

            // Authorization: Users can view their own activity; Admins/Managers can view any user's activity
            if (!isAdmin && !isManager && request.UserId != request.RequestorUserId)
            {
                throw new UnauthorizedAccessException("You can only view your own activity history.");
            }

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? _historySettings.DefaultPageSize : request.PageSize;
            pageSize = Math.Min(pageSize, _historySettings.MaxPageSize);

            var query = _db.ClientHistories.AsNoTracking()
                .Where(h => h.ActorUserId == request.UserId);

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

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(h => h.CreatedAt)
                .ThenByDescending(h => h.HistoryId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(h => h.ActorUser)
                .Include(h => h.Client)
                .ProjectTo<ClientHistoryEntryDto>(_mapper.ConfigurationProvider)
                .ToArrayAsync();

            return new PagedResult<ClientHistoryEntryDto>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
}

