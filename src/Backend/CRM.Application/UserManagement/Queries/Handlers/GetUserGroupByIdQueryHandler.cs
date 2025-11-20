using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Queries;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Queries.Handlers;

public class GetUserGroupByIdQueryHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetUserGroupByIdQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<UserGroupDto> Handle(GetUserGroupByIdQuery query)
    {
        var group = await _db.UserGroups
            .AsNoTracking()
            .Include(ug => ug.CreatedByUser)
            .Include(ug => ug.Members)
            .FirstOrDefaultAsync(ug => ug.GroupId == query.GroupId);

        if (group == null)
        {
            throw new UserGroupNotFoundException(query.GroupId);
        }

        var dto = new UserGroupDto
        {
            GroupId = group.GroupId,
            Name = group.Name,
            Description = group.Description,
            Permissions = group.GetPermissions(),
            CreatedByUserId = group.CreatedByUserId,
            CreatedByUserName = group.CreatedByUser?.GetFullName() ?? string.Empty,
            MemberCount = group.Members.Count,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt
        };

        return dto;
    }
}

