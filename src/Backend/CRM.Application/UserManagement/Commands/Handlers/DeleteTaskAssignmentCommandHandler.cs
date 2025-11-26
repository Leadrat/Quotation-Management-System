using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class DeleteTaskAssignmentCommandHandler
{
    private readonly IAppDbContext _db;

    public DeleteTaskAssignmentCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteTaskAssignmentCommand cmd)
    {
        var assignment = await _db.TaskAssignments
            .FirstOrDefaultAsync(ta => ta.AssignmentId == cmd.AssignmentId);

        if (assignment == null)
        {
            throw new TaskAssignmentNotFoundException(cmd.AssignmentId);
        }

        // Authorization: Only Admin, Manager, Team Lead, or the assigner can delete
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(cmd.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(cmd.RequestorRole, "TeamLead", StringComparison.OrdinalIgnoreCase) ||
                          assignment.AssignedByUserId == cmd.DeletedByUserId;
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Not authorized to delete this task assignment");
        }

        _db.TaskAssignments.Remove(assignment);
        await _db.SaveChangesAsync();
    }
}

