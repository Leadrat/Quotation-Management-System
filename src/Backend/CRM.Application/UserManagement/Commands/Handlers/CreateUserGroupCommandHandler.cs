using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using CRM.Domain.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class CreateUserGroupCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public CreateUserGroupCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<UserGroupDto> Handle(CreateUserGroupCommand cmd)
    {
        // Authorization: Only Admin can create user groups
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin can create user groups");
        }

        // Validate creator exists
        var creator = await _db.Users.FirstOrDefaultAsync(u => u.UserId == cmd.CreatedByUserId);
        if (creator == null || !creator.IsActive || creator.DeletedAt != null)
        {
            throw new InvalidOperationException("Creator user not found or inactive");
        }

        var now = DateTime.UtcNow;
        var group = new UserGroup
        {
            GroupId = Guid.NewGuid(),
            Name = cmd.Request.Name.Trim(),
            Description = cmd.Request.Description?.Trim(),
            CreatedByUserId = cmd.CreatedByUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Set permissions
        if (cmd.Request.Permissions != null && cmd.Request.Permissions.Any())
        {
            group.SetPermissions(cmd.Request.Permissions);
        }
        else
        {
            group.SetPermissions(new List<string>());
        }

        _db.UserGroups.Add(group);
        await _db.SaveChangesAsync();

        // Load with navigation properties
        var groupWithNav = await _db.UserGroups
            .Include(ug => ug.CreatedByUser)
            .Include(ug => ug.Members)
            .FirstOrDefaultAsync(ug => ug.GroupId == group.GroupId);

        var dto = _mapper.Map<UserGroupDto>(groupWithNav);
        dto.MemberCount = groupWithNav?.Members.Count ?? 0;

        return dto;
    }
}

