using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CRM.Application.Roles.Queries;
using CRM.Domain.Entities;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Roles.Queries.Handlers;

public class GetAllRolesQueryHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetAllRolesQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<object> Handle(GetAllRolesQuery query)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize <= 0 ? 10 : query.PageSize, 1, 100);

        var roles = _db.Roles.AsQueryable();
        if (query.IsActive.HasValue)
        {
            roles = roles.Where(r => r.IsActive == query.IsActive.Value);
        }

        var totalCount = await roles.CountAsync();

        var list = await roles
            .OrderBy(r => r.RoleName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                Description = r.Description,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                UserCount = _db.Users.Count(u => u.RoleId == r.RoleId && u.DeletedAt == null && u.IsActive)
            })
            .ToListAsync();

        return new
        {
            success = true,
            data = list,
            pageNumber,
            pageSize,
            totalCount
        };
    }
}
