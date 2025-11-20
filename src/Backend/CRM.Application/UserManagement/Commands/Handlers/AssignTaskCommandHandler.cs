using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using CRM.Domain.UserManagement;
using CRM.Domain.UserManagement.Events;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class AssignTaskCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public AssignTaskCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<TaskAssignmentDto> Handle(AssignTaskCommand cmd)
    {
        // Authorization: Only Manager or Team Lead can assign tasks
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(cmd.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(cmd.RequestorRole, "TeamLead", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin, Manager, or Team Lead can assign tasks");
        }

        // Validate assigned to user exists and is active
        var assignedToUser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == cmd.Request.AssignedToUserId);
        if (assignedToUser == null || !assignedToUser.IsActive || assignedToUser.DeletedAt != null)
        {
            throw new InvalidOperationException("Assigned to user not found or inactive");
        }

        // Validate entity type
        var validEntityTypes = new[] { "Quotation", "Approval", "Client" };
        if (!validEntityTypes.Contains(cmd.Request.EntityType))
        {
            throw new InvalidOperationException($"Invalid entity type. Must be one of: {string.Join(", ", validEntityTypes)}");
        }

        // TODO: Validate entity exists (check Quotations, DiscountApprovals, Clients tables based on EntityType)

        var now = DateTime.UtcNow;
        var assignment = new TaskAssignment
        {
            AssignmentId = Guid.NewGuid(),
            EntityType = cmd.Request.EntityType,
            EntityId = cmd.Request.EntityId,
            AssignedToUserId = cmd.Request.AssignedToUserId,
            AssignedByUserId = cmd.AssignedByUserId,
            DueDate = cmd.Request.DueDate,
            Status = TaskAssignmentStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.TaskAssignments.Add(assignment);
        await _db.SaveChangesAsync();

        // Load with navigation properties
        var assignmentWithNav = await _db.TaskAssignments
            .Include(ta => ta.AssignedToUser)
            .Include(ta => ta.AssignedByUser)
            .FirstOrDefaultAsync(ta => ta.AssignmentId == assignment.AssignmentId);

        var dto = _mapper.Map<TaskAssignmentDto>(assignmentWithNav);
        dto.IsOverdue = assignment.IsOverdue();

        return dto;
    }
}

