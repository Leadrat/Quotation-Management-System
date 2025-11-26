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

public class AddUserGroupMemberCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public AddUserGroupMemberCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<UserGroupMemberDto> Handle(AddUserGroupMemberCommand cmd)
    {
        // Authorization: Only Admin can add members to groups
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin can add members to user groups");
        }

        var group = await _db.UserGroups
            .FirstOrDefaultAsync(ug => ug.GroupId == cmd.GroupId);

        if (group == null)
        {
            throw new UserGroupNotFoundException(cmd.GroupId);
        }

        // Validate user exists and is active
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == cmd.UserId);
        if (user == null || !user.IsActive || user.DeletedAt != null)
        {
            throw new InvalidOperationException("User not found or inactive");
        }

        // Check if user is already a member
        var existingMember = await _db.UserGroupMembers
            .FirstOrDefaultAsync(ugm => ugm.GroupId == cmd.GroupId && ugm.UserId == cmd.UserId);
        if (existingMember != null)
        {
            throw new InvalidOperationException("User is already a member of this group");
        }

        var now = DateTime.UtcNow;
        var member = new UserGroupMember
        {
            GroupMemberId = Guid.NewGuid(),
            GroupId = cmd.GroupId,
            UserId = cmd.UserId,
            AddedAt = now
        };

        _db.UserGroupMembers.Add(member);
        await _db.SaveChangesAsync();

        // Load with navigation properties
        var memberWithNav = await _db.UserGroupMembers
            .Include(ugm => ugm.UserGroup)
            .Include(ugm => ugm.User)
            .FirstOrDefaultAsync(ugm => ugm.GroupMemberId == member.GroupMemberId);

        return _mapper.Map<UserGroupMemberDto>(memberWithNav);
    }
}

