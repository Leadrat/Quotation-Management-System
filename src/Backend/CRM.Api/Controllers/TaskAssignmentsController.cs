using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.Commands.Handlers;
using CRM.Application.UserManagement.Queries;
using CRM.Application.UserManagement.Queries.Handlers;
using CRM.Application.UserManagement.Requests;
using CRM.Application.UserManagement.Validators;
using CRM.Application.Common.Results;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/v1/task-assignments")]
[Authorize]
public class TaskAssignmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly IMapper _mapper;

    public TaskAssignmentsController(AppDbContext db, IAuditLogger audit, IMapper mapper)
    {
        _db = db;
        _audit = audit;
        _mapper = mapper;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Assign([FromBody] AssignTaskRequest body)
    {
        var validator = new AssignTaskRequestValidator();
        var result = validator.Validate(body);
        if (!result.IsValid)
        {
            return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("task_assign_attempt", new { userId, body.EntityType, body.EntityId, body.AssignedToUserId });

        var cmd = new AssignTaskCommand
        {
            Request = body,
            AssignedByUserId = userId,
            RequestorRole = role
        };

        var handler = new AssignTaskCommandHandler(_db, _mapper);
        try
        {
            var assignment = await handler.Handle(cmd);
            await _audit.LogAsync("task_assign_success", new { userId, assignment.AssignmentId });
            return StatusCode(201, new { success = true, message = "Task assigned successfully", data = assignment });
        }
        catch (CRM.Application.UserManagement.Exceptions.UnauthorizedTeamOperationException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserTasks([FromRoute] Guid userId, 
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null, [FromQuery] string? entityType = null,
        [FromQuery] DateTime? dueDateFrom = null, [FromQuery] DateTime? dueDateTo = null)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        var handler = new GetUserTasksQueryHandler(_db, _mapper);
        try
        {
            var result = await handler.Handle(new GetUserTasksQuery
            {
                UserId = userId,
                Status = status,
                EntityType = entityType,
                DueDateFrom = dueDateFrom,
                DueDateTo = dueDateTo,
                PageNumber = pageNumber,
                PageSize = pageSize,
                RequestorUserId = requestorId,
                RequestorRole = role
            });
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{assignmentId}/status")]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid assignmentId, [FromBody] UpdateTaskStatusRequest body)
    {
        var validator = new UpdateTaskStatusRequestValidator();
        var result = validator.Validate(body);
        if (!result.IsValid)
        {
            return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

        await _audit.LogAsync("task_status_update_attempt", new { userId, assignmentId, body.Status });

        var cmd = new UpdateTaskStatusCommand
        {
            AssignmentId = assignmentId,
            Request = body,
            UpdatedByUserId = userId
        };

        var handler = new UpdateTaskStatusCommandHandler(_db, _mapper);
        try
        {
            var updated = await handler.Handle(cmd);
            await _audit.LogAsync("task_status_update_success", new { userId, assignmentId, body.Status });
            return Ok(new { success = true, message = "Task status updated successfully", data = updated });
        }
        catch (CRM.Application.UserManagement.Exceptions.TaskAssignmentNotFoundException)
        {
            return NotFound(new { success = false, error = "Task assignment not found" });
        }
        catch (CRM.Application.UserManagement.Exceptions.UnauthorizedTeamOperationException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("{assignmentId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete([FromRoute] Guid assignmentId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("task_delete_attempt", new { userId, assignmentId });

        var cmd = new DeleteTaskAssignmentCommand
        {
            AssignmentId = assignmentId,
            DeletedByUserId = userId,
            RequestorRole = role
        };

        var handler = new DeleteTaskAssignmentCommandHandler(_db);
        try
        {
            await handler.Handle(cmd);
            await _audit.LogAsync("task_delete_success", new { userId, assignmentId });
            return Ok(new { success = true, message = "Task assignment deleted successfully" });
        }
        catch (CRM.Application.UserManagement.Exceptions.TaskAssignmentNotFoundException)
        {
            return NotFound(new { success = false, error = "Task assignment not found" });
        }
        catch (CRM.Application.UserManagement.Exceptions.UnauthorizedTeamOperationException ex)
        {
            return Forbid(ex.Message);
        }
    }
}

