using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.Clients.Dtos;
using CRM.Application.Clients.Exceptions;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CRM.Application.Clients.Queries.Handlers
{
    public class GetClientHistoryQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly HistorySettings _historySettings;

        public GetClientHistoryQueryHandler(IAppDbContext db, IMapper mapper, IOptions<HistorySettings> historySettings)
        {
            _db = db;
            _mapper = mapper;
            _historySettings = historySettings.Value;
        }

        public async Task<PagedResult<ClientHistoryEntryDto>> Handle(GetClientHistoryQuery request)
        {
            var clientSummary = await _db.Clients
                .AsNoTracking()
                .Where(c => c.ClientId == request.ClientId)
                .Select(c => new { c.ClientId, c.CreatedByUserId })
                .FirstOrDefaultAsync();

            if (clientSummary == null)
            {
                throw new ClientNotFoundException(request.ClientId);
            }

            var isAdmin = string.Equals(request.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && clientSummary.CreatedByUserId != request.RequestorUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this client's history.");
            }

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? _historySettings.DefaultPageSize : request.PageSize;
            pageSize = Math.Min(pageSize, _historySettings.MaxPageSize);

            var historyQuery = _db.ClientHistories.AsNoTracking()
                .Where(h => h.ClientId == request.ClientId);

            if (!request.IncludeAccessLogs)
            {
                historyQuery = historyQuery.Where(h => h.ActionType != "ACCESSED");
            }

            var total = await historyQuery.CountAsync();

            var data = await historyQuery
                .OrderByDescending(h => h.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(h => h.ActorUser)
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

