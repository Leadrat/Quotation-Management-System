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

public class UpdateTaskStatusCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public UpdateTaskStatusCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<TaskAssignmentDto> Handle(UpdateTaskStatusCommand cmd)
    {
        var assignment = await _db.TaskAssignments
            .Include(ta => ta.AssignedToUser)
            .Include(ta => ta.AssignedByUser)
            .FirstOrDefaultAsync(ta => ta.AssignmentId == cmd.AssignmentId);

        if (assignment == null)
        {
            throw new TaskAssignmentNotFoundException(cmd.AssignmentId);
        }

        // Authorization: Only the assigned user or the assigner can update status
        if (assignment.AssignedToUserId != cmd.UpdatedByUserId && assignment.AssignedByUserId != cmd.UpdatedByUserId)
        {
            throw new UnauthorizedTeamOperationException("Only the assigned user or the assigner can update task status");
        }

        // Validate status
        var validStatuses = new[] { "Pending", "InProgress", "Completed", "Cancelled" };
        if (!validStatuses.Contains(cmd.Request.Status))
        {
            throw new InvalidOperationException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");
        }

        // Update status using domain methods
        var statusEnum = cmd.Request.Status switch
        {
            "Pending" => TaskAssignmentStatus.Pending,
            "InProgress" => TaskAssignmentStatus.InProgress,
            "Completed" => TaskAssignmentStatus.Completed,
            "Cancelled" => TaskAssignmentStatus.Cancelled,
            _ => throw new InvalidOperationException("Invalid status")
        };

        // Use domain methods to update status (they handle UpdatedAt)
        if (statusEnum == TaskAssignmentStatus.Completed)
        {
            assignment.MarkAsCompleted();
        }
        else if (statusEnum == TaskAssignmentStatus.InProgress)
        {
            assignment.MarkAsInProgress();
        }
        else if (statusEnum == TaskAssignmentStatus.Cancelled)
        {
            assignment.Cancel();
        }
        else if (statusEnum == TaskAssignmentStatus.Pending)
        {
            assignment.Status = TaskAssignmentStatus.Pending;
            assignment.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var updatedAssignment = await _db.TaskAssignments
            .Include(ta => ta.AssignedToUser)
            .Include(ta => ta.AssignedByUser)
            .FirstOrDefaultAsync(ta => ta.AssignmentId == assignment.AssignmentId);

        var dto = _mapper.Map<TaskAssignmentDto>(updatedAssignment);
        dto.IsOverdue = updatedAssignment?.IsOverdue() ?? false;

        return dto;
    }
}

