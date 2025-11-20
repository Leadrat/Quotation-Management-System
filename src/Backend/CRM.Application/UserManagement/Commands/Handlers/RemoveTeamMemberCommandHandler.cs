using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class RemoveTeamMemberCommandHandler
{
    private readonly IAppDbContext _db;

    public RemoveTeamMemberCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(RemoveTeamMemberCommand cmd)
    {
        var team = await _db.Teams
            .Include(t => t.TeamLead)
            .FirstOrDefaultAsync(t => t.TeamId == cmd.TeamId && t.IsActive);

        if (team == null)
        {
            throw new TeamNotFoundException(cmd.TeamId);
        }

        // Authorization: Only Admin, Manager, or Team Lead can remove members
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(cmd.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase) ||
                          team.TeamLeadUserId == cmd.RemovedByUserId;
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin, Manager, or Team Lead can remove team members");
        }

        var teamMember = await _db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId);

        if (teamMember == null)
        {
            throw new InvalidOperationException("User is not a member of this team");
        }

        // Prevent removing the team lead
        if (team.TeamLeadUserId == cmd.UserId)
        {
            throw new InvalidOperationException("Cannot remove team lead. Update team lead first.");
        }

        _db.TeamMembers.Remove(teamMember);
        await _db.SaveChangesAsync();
    }
}

