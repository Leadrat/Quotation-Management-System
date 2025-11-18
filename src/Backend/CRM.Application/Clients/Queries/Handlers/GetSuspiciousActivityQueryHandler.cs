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
    public class GetSuspiciousActivityQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly HistorySettings _historySettings;

        public GetSuspiciousActivityQueryHandler(IAppDbContext db, IMapper mapper, IOptions<HistorySettings> historySettings)
        {
            _db = db;
            _mapper = mapper;
            _historySettings = historySettings.Value;
        }

        public async Task<PagedResult<SuspiciousActivityDto>> Handle(GetSuspiciousActivityQuery request)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? _historySettings.DefaultPageSize : request.PageSize;
            pageSize = Math.Min(pageSize, _historySettings.MaxPageSize);

            var query = _db.SuspiciousActivityFlags.AsNoTracking()
                .Where(f => f.Score >= request.MinScore);

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(f => f.Status == request.Status);
            }

            if (request.DateFrom.HasValue)
            {
                query = query.Where(f => f.DetectedAt >= request.DateFrom.Value);
            }

            if (request.DateTo.HasValue)
            {
                query = query.Where(f => f.DetectedAt <= request.DateTo.Value);
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(f => f.Score)
                .ThenByDescending(f => f.DetectedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(f => f.History)
                .ThenInclude(h => h!.ActorUser)
                .Include(f => f.History)
                .ThenInclude(h => h!.Client)
                .ProjectTo<SuspiciousActivityDto>(_mapper.ConfigurationProvider)
                .ToArrayAsync();

            return new PagedResult<SuspiciousActivityDto>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
}

