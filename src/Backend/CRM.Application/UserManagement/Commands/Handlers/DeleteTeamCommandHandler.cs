using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class DeleteTeamCommandHandler
{
    private readonly IAppDbContext _db;

    public DeleteTeamCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteTeamCommand cmd)
    {
        var team = await _db.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.TeamId == cmd.TeamId);

        if (team == null)
        {
            throw new TeamNotFoundException(cmd.TeamId);
        }

        // Authorization: Only Admin or Manager can delete teams
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(cmd.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin or Manager can delete teams");
        }

        // Soft delete: mark as inactive
        team.IsActive = false;
        team.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}

