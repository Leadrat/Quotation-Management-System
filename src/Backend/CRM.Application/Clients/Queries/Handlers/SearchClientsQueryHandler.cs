using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.Clients.Dtos;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Queries.Handlers
{
    public class SearchClientsQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        public SearchClientsQueryHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PagedResult<ClientDto>> Handle(SearchClientsQuery q)
        {
            var pageNumber = q.PageNumber < 1 ? 1 : q.PageNumber;
            var pageSize = q.PageSize > 100 ? 100 : (q.PageSize < 1 ? 10 : q.PageSize);

            var query = _db.Clients.AsNoTracking();
            if (!q.IncludeDeleted)
            {
                query = query.Where(c => c.DeletedAt == null);
            }

            // Authorization scope
            var isAdmin = string.Equals(q.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
            {
                query = query.Where(c => c.CreatedByUserId == q.RequestorUserId);
            }
            else if (q.CreatedByUserId.HasValue)
            {
                query = query.Where(c => c.CreatedByUserId == q.CreatedByUserId.Value);
            }

            // Filters
            if (!string.IsNullOrWhiteSpace(q.City))
            {
                var city = q.City.Trim().ToLower();
                query = query.Where(c => c.City != null && c.City.ToLower().Contains(city));
            }
            if (!string.IsNullOrWhiteSpace(q.State))
            {
                var state = q.State.Trim().ToLower();
                query = query.Where(c => c.State != null && c.State.ToLower().Contains(state));
            }
            if (!string.IsNullOrWhiteSpace(q.StateCode))
            {
                query = query.Where(c => c.StateCode == q.StateCode);
            }
            if (!string.IsNullOrWhiteSpace(q.Gstin))
            {
                var gst = q.Gstin.Trim();
                query = query.Where(c => c.Gstin != null && c.Gstin.Contains(gst));
            }
            if (q.CreatedDateFrom.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= q.CreatedDateFrom.Value);
            }
            if (q.CreatedDateTo.HasValue)
            {
                query = query.Where(c => c.CreatedAt < q.CreatedDateTo.Value);
            }
            if (q.UpdatedDateFrom.HasValue)
            {
                query = query.Where(c => c.UpdatedAt >= q.UpdatedDateFrom.Value);
            }
            if (q.UpdatedDateTo.HasValue)
            {
                query = query.Where(c => c.UpdatedAt < q.UpdatedDateTo.Value);
            }

            // Search term hybrid strategy (approximation in LINQ)
            if (!string.IsNullOrWhiteSpace(q.SearchTerm))
            {
                var term = q.SearchTerm.Trim();
                // For short terms (<3) or as fallback, use partial matches
                var lower = term.ToLower();
                query = query.Where(c =>
                    (c.CompanyName != null && c.CompanyName.ToLower().Contains(lower)) ||
                    (c.ContactName != null && c.ContactName.ToLower().Contains(lower)) ||
                    (c.Email != null && c.Email.ToLower().Contains(lower))
                );
            }

            // Sorting + stable tiebreakers
            query = q.SortBy switch
            {
                "NameAsc" => query.OrderBy(c => c.CompanyName).ThenByDescending(c => c.CreatedAt).ThenBy(c => c.ClientId),
                "NameDesc" => query.OrderByDescending(c => c.CompanyName).ThenByDescending(c => c.CreatedAt).ThenBy(c => c.ClientId),
                "CreatedAtAsc" => query.OrderBy(c => c.CreatedAt).ThenBy(c => c.ClientId),
                "CreatedAtDesc" => query.OrderByDescending(c => c.CreatedAt).ThenBy(c => c.ClientId),
                "UpdatedAtDesc" => query.OrderByDescending(c => c.UpdatedAt).ThenBy(c => c.ClientId),
                "EmailAsc" => query.OrderBy(c => c.Email).ThenByDescending(c => c.CreatedAt).ThenBy(c => c.ClientId),
                _ => query.OrderByDescending(c => c.CreatedAt).ThenBy(c => c.ClientId)
            };

            var total = await query.CountAsync();

            var data = await query
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
