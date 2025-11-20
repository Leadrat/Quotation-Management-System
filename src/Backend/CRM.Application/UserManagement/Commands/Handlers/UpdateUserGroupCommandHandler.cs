using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class UpdateUserGroupCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public UpdateUserGroupCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<UserGroupDto> Handle(UpdateUserGroupCommand cmd)
    {
        // Authorization: Only Admin can update user groups
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin can update user groups");
        }

        var group = await _db.UserGroups
            .Include(ug => ug.CreatedByUser)
            .Include(ug => ug.Members)
            .FirstOrDefaultAsync(ug => ug.GroupId == cmd.GroupId);

        if (group == null)
        {
            throw new UserGroupNotFoundException(cmd.GroupId);
        }

        if (!string.IsNullOrWhiteSpace(cmd.Request.Name))
        {
            group.Name = cmd.Request.Name.Trim();
        }

        if (cmd.Request.Description != null)
        {
            group.Description = cmd.Request.Description.Trim();
        }

        if (cmd.Request.Permissions != null)
        {
            group.SetPermissions(cmd.Request.Permissions);
        }

        group.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var updatedGroup = await _db.UserGroups
            .Include(ug => ug.CreatedByUser)
            .Include(ug => ug.Members)
            .FirstOrDefaultAsync(ug => ug.GroupId == group.GroupId);

        var dto = _mapper.Map<UserGroupDto>(updatedGroup);
        dto.MemberCount = updatedGroup?.Members.Count ?? 0;

        return dto;
    }
}

