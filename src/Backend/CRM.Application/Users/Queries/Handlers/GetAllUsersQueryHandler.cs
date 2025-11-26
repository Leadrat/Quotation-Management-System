using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using CRM.Shared.Constants;

namespace CRM.Application.Users.Queries.Handlers
{
    public class GetAllUsersQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ITenantContext _tenantContext;

        public GetAllUsersQueryHandler(IAppDbContext db, IMapper mapper, ITenantContext tenantContext)
        {
            _db = db;
            _mapper = mapper;
            _tenantContext = tenantContext;
        }

        public async Task<PagedResult<UserDto>> Handle(GetAllUsersQuery q)
        {
            var pageNumber = q.PageNumber < 1 ? 1 : q.PageNumber;
            var pageSize = q.PageSize > 100 ? 100 : (q.PageSize < 1 ? 10 : q.PageSize);
            // Filter by current tenant (include NULL for legacy data)
            var currentTenantId = _tenantContext.CurrentTenantId;
            var query = _db.Users.AsNoTracking()
                .Include(u => u.Role)
                .Where(u => u.DeletedAt == null && 
                    (u.Role.RoleName == "SuperAdmin" || u.TenantId == currentTenantId || u.TenantId == null));

            // Exclude Client role users - only Admin, SalesRep, and Manager should be in Users management
            query = query.Where(u => u.RoleId != RoleIds.Client);

            // Only Admin can list all users, others see only themselves
            var isAdmin = string.Equals(q.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
            {
                query = query.Where(u => u.UserId == q.RequestorUserId);
            }

            // Apply search term if provided
            if (!string.IsNullOrWhiteSpace(q.SearchTerm))
            {
                var searchTerm = q.SearchTerm.Trim().ToLower();
                query = query.Where(u =>
                    (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(searchTerm)) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchTerm))
                );
            }

            var total = await query.CountAsync();
            var data = await query
                .Include(u => u.Role)
                .Include(u => u.ReportingManager)
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                .ToArrayAsync();

            return new PagedResult<UserDto>
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

