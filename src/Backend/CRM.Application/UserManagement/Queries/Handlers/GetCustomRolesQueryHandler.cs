using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Queries;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Queries.Handlers;

public class GetCustomRolesQueryHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetCustomRolesQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResult<CustomRoleDto>> Handle(GetCustomRolesQuery query)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize > 100 ? 100 : (query.PageSize < 1 ? 10 : query.PageSize);

        var rolesQuery = _db.Roles
            .AsNoTracking()
            .Include(r => r.UserRoles)
            .AsQueryable();

        // Filter by built-in status
        if (!query.IncludeBuiltIn.HasValue || !query.IncludeBuiltIn.Value)
        {
            rolesQuery = rolesQuery.Where(r => !r.IsBuiltIn);
        }

        // Filter by active status
        if (query.IsActive.HasValue)
        {
            rolesQuery = rolesQuery.Where(r => r.IsActive == query.IsActive.Value);
        }

        var total = await rolesQuery.CountAsync();

        var roles = await rolesQuery
            .OrderBy(r => r.RoleName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = roles.Select(r => new CustomRoleDto
        {
            RoleId = r.RoleId,
            RoleName = r.RoleName,
            Description = r.Description,
            Permissions = r.GetPermissions(),
            IsBuiltIn = r.IsBuiltIn,
            IsActive = r.IsActive,
            UserCount = r.UserRoles.Count,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToArray();

        return new PagedResult<CustomRoleDto>
        {
            Success = true,
            Data = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}

