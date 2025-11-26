using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class RemoveUserGroupMemberCommandHandler
{
    private readonly IAppDbContext _db;

    public RemoveUserGroupMemberCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(RemoveUserGroupMemberCommand cmd)
    {
        // Authorization: Only Admin can remove members from groups
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin can remove members from user groups");
        }

        var group = await _db.UserGroups
            .FirstOrDefaultAsync(ug => ug.GroupId == cmd.GroupId);

        if (group == null)
        {
            throw new UserGroupNotFoundException(cmd.GroupId);
        }

        var member = await _db.UserGroupMembers
            .FirstOrDefaultAsync(ugm => ugm.GroupId == cmd.GroupId && ugm.UserId == cmd.UserId);

        if (member == null)
        {
            throw new InvalidOperationException("User is not a member of this group");
        }

        _db.UserGroupMembers.Remove(member);
        await _db.SaveChangesAsync();
    }
}

