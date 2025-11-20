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

public class GetUserGroupsQueryHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetUserGroupsQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserGroupDto>> Handle(GetUserGroupsQuery query)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize > 100 ? 100 : (query.PageSize < 1 ? 10 : query.PageSize);

        var groupsQuery = _db.UserGroups
            .AsNoTracking()
            .Include(ug => ug.CreatedByUser)
            .Include(ug => ug.Members)
            .AsQueryable();

        // Filter by creator if provided
        if (query.CreatedByUserId.HasValue)
        {
            groupsQuery = groupsQuery.Where(ug => ug.CreatedByUserId == query.CreatedByUserId.Value);
        }

        var total = await groupsQuery.CountAsync();

        var groups = await groupsQuery
            .OrderBy(ug => ug.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = groups.Select(g => new UserGroupDto
        {
            GroupId = g.GroupId,
            Name = g.Name,
            Description = g.Description,
            Permissions = g.GetPermissions(),
            CreatedByUserId = g.CreatedByUserId,
            CreatedByUserName = g.CreatedByUser?.GetFullName() ?? string.Empty,
            MemberCount = g.Members.Count,
            CreatedAt = g.CreatedAt,
            UpdatedAt = g.UpdatedAt
        }).ToArray();

        return new PagedResult<UserGroupDto>
        {
            Success = true,
            Data = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}

