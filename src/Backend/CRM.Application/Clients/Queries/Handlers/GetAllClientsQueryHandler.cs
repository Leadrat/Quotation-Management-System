using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.Clients.Dtos;
using CRM.Application.Clients.Queries;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Queries.Handlers
{
    public class GetAllClientsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        public GetAllClientsQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PagedResult<ClientDto>> Handle(GetAllClientsQuery q)
        {
            var pageNumber = q.PageNumber < 1 ? 1 : q.PageNumber;
            var pageSize = q.PageSize > 100 ? 100 : (q.PageSize < 1 ? 10 : q.PageSize);

            var query = _db.Clients.AsNoTracking().Where(c => c.DeletedAt == null);

            var isAdmin = string.Equals(q.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
            {
                query = query.Where(c => c.CreatedByUserId == q.RequestorUserId);
            }
            else if (q.CreatedByUserId.HasValue)
            {
                query = query.Where(c => c.CreatedByUserId == q.CreatedByUserId.Value);
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.CreatedByUser)
                .ProjectTo<ClientDto>(_mapper.ConfigurationProvider)
                .ToArrayAsync();

            return new PagedResult<ClientDto>
            {
                Success = true,
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
}
