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
[Route("api/v1/user-groups")]
[Authorize]
public class UserGroupsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly IMapper _mapper;

    public UserGroupsController(AppDbContext db, IAuditLogger audit, IMapper mapper)
    {
        _db = db;
        _audit = audit;
        _mapper = mapper;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserGroupRequest body)
    {
        var validator = new CreateUserGroupRequestValidator();
        var result = validator.Validate(body);
        if (!result.IsValid)
        {
            return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("user_group_create_attempt", new { userId, body.Name });

        var cmd = new CreateUserGroupCommand
        {
            Request = body,
            CreatedByUserId = userId,
            RequestorRole = role
        };

        var handler = new CreateUserGroupCommandHandler(_db, _mapper);
        try
        {
            var created = await handler.Handle(cmd);
            await _audit.LogAsync("user_group_create_success", new { userId, created.GroupId });
            return StatusCode(201, new { success = true, message = "User group created successfully", data = created });
        }
        catch (CRM.Application.UserManagement.Exceptions.UnauthorizedTeamOperationException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> List([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, 
        [FromQuery] Guid? createdByUserId = null)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        var handler = new GetUserGroupsQueryHandler(_db, _mapper);
        var result = await handler.Handle(new GetUserGroupsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            CreatedByUserId = createdByUserId,
            RequestorUserId = requestorId,
            RequestorRole = role
        });
        return Ok(result);
    }

    [HttpGet("{groupId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById([FromRoute] Guid groupId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        var handler = new GetUserGroupByIdQueryHandler(_db, _mapper);
        try
        {
            var result = await handler.Handle(new GetUserGroupByIdQuery
            {
                GroupId = groupId,
                RequestorUserId = requestorId,
                RequestorRole = role
            });
            return Ok(new { success = true, data = result });
        }
        catch (CRM.Application.UserManagement.Exceptions.UserGroupNotFoundException)
        {
            return NotFound(new { success = false, error = "User group not found" });
        }
    }

    [HttpPut("{groupId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromRoute] Guid groupId, [FromBody] UpdateUserGroupRequest body)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("user_group_update_attempt", new { userId, groupId });

        var cmd = new UpdateUserGroupCommand
        {
            GroupId = groupId,
            Request = body,
            UpdatedByUserId = userId,
            RequestorRole = role
        };

        var handler = new UpdateUserGroupCommandHandler(_db, _mapper);
        try
        {
            var updated = await handler.Handle(cmd);
            await _audit.LogAsync("user_group_update_success", new { userId, groupId });
            return Ok(new { success = true, message = "User group updated successfully", data = updated });
        }
        catch (CRM.Application.UserManagement.Exceptions.UserGroupNotFoundException)
        {
            return NotFound(new { success = false, error = "User group not found" });
        }
        catch (CRM.Application.UserManagement.Exceptions.UnauthorizedTeamOperationException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("{groupId}/members")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddMember([FromRoute] Guid groupId, [FromBody] Guid userId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("user_group_member_add_attempt", new { requestorId, groupId, userId });

        var cmd = new AddUserGroupMemberCommand
        {
            GroupId = groupId,
            UserId = userId,
            AddedByUserId = requestorId,
            RequestorRole = role
        };

        var handler = new AddUserGroupMemberCommandHandler(_db, _mapper);
        try
        {
            var member = await handler.Handle(cmd);
            await _audit.LogAsync("user_group_member_add_success", new { requestorId, groupId, userId });
            return StatusCode(201, new { success = true, message = "User added to group successfully", data = member });
        }
        catch (CRM.Application.UserManagement.Exceptions.UserGroupNotFoundException)
        {
            return NotFound(new { success = false, error = "User group not found" });
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

    [HttpDelete("{groupId}/members/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveMember([FromRoute] Guid groupId, [FromRoute] Guid userId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("user_group_member_remove_attempt", new { requestorId, groupId, userId });

        var cmd = new RemoveUserGroupMemberCommand
        {
            GroupId = groupId,
            UserId = userId,
            RemovedByUserId = requestorId,
            RequestorRole = role
        };

        var handler = new RemoveUserGroupMemberCommandHandler(_db);
        try
        {
            await handler.Handle(cmd);
            await _audit.LogAsync("user_group_member_remove_success", new { requestorId, groupId, userId });
            return Ok(new { success = true, message = "User removed from group successfully" });
        }
        catch (CRM.Application.UserManagement.Exceptions.UserGroupNotFoundException)
        {
            return NotFound(new { success = false, error = "User group not found" });
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
}

