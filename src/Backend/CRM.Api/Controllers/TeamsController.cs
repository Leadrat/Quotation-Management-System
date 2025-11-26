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
[Route("api/v1/teams")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly IMapper _mapper;

    public TeamsController(AppDbContext db, IAuditLogger audit, IMapper mapper)
    {
        _db = db;
        _audit = audit;
        _mapper = mapper;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequest body)
    {
        var validator = new CreateTeamRequestValidator();
        var result = validator.Validate(body);
        if (!result.IsValid)
        {
            return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("team_create_attempt", new { userId, body.Name });

        var cmd = new CreateTeamCommand
        {
            Request = body,
            CreatedByUserId = userId,
            RequestorRole = role
        };

        var handler = new CreateTeamCommandHandler(_db, _mapper);
        var created = await handler.Handle(cmd);

        await _audit.LogAsync("team_create_success", new { userId, created.TeamId });
        return StatusCode(201, new { success = true, message = "Team created successfully", data = created });
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, 
        [FromQuery] Guid? companyId = null, [FromQuery] Guid? teamLeadUserId = null, 
        [FromQuery] bool? isActive = null, [FromQuery] Guid? parentTeamId = null)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        var handler = new GetTeamsQueryHandler(_db, _mapper);
        var result = await handler.Handle(new GetTeamsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            CompanyId = companyId,
            TeamLeadUserId = teamLeadUserId,
            IsActive = isActive,
            ParentTeamId = parentTeamId,
            RequestorUserId = requestorId,
            RequestorRole = role
        });
        return Ok(result);
    }

    [HttpGet("{teamId}")]
    public async Task<IActionResult> GetById([FromRoute] Guid teamId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        var handler = new GetTeamByIdQueryHandler(_db, _mapper);
        try
        {
            var result = await handler.Handle(new GetTeamByIdQuery
            {
                TeamId = teamId,
                RequestorUserId = requestorId,
                RequestorRole = role
            });
            return Ok(new { success = true, data = result });
        }
        catch (CRM.Application.UserManagement.Exceptions.TeamNotFoundException)
        {
            return NotFound(new { success = false, error = "Team not found" });
        }
    }

    [HttpPut("{teamId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update([FromRoute] Guid teamId, [FromBody] UpdateTeamRequest body)
    {
        var validator = new UpdateTeamRequestValidator();
        var result = validator.Validate(body);
        if (!result.IsValid)
        {
            return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("team_update_attempt", new { userId, teamId });

        var cmd = new UpdateTeamCommand
        {
            TeamId = teamId,
            Request = body,
            UpdatedByUserId = userId,
            RequestorRole = role
        };

        var handler = new UpdateTeamCommandHandler(_db, _mapper);
        try
        {
            var updated = await handler.Handle(cmd);
            await _audit.LogAsync("team_update_success", new { userId, teamId });
            return Ok(new { success = true, message = "Team updated successfully", data = updated });
        }
        catch (CRM.Application.UserManagement.Exceptions.TeamNotFoundException)
        {
            return NotFound(new { success = false, error = "Team not found" });
        }
        catch (CRM.Application.UserManagement.Exceptions.UnauthorizedTeamOperationException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{teamId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete([FromRoute] Guid teamId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("team_delete_attempt", new { userId, teamId });

        var cmd = new DeleteTeamCommand
        {
            TeamId = teamId,
            DeletedByUserId = userId,
            RequestorRole = role
        };

        var handler = new DeleteTeamCommandHandler(_db);
        try
        {
            await handler.Handle(cmd);
            await _audit.LogAsync("team_delete_success", new { userId, teamId });
            return Ok(new { success = true, message = "Team deleted successfully" });
        }
        catch (CRM.Application.UserManagement.Exceptions.TeamNotFoundException)
        {
            return NotFound(new { success = false, error = "Team not found" });
        }
        catch (CRM.Application.UserManagement.Exceptions.UnauthorizedTeamOperationException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("{teamId}/members")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddMember([FromRoute] Guid teamId, [FromBody] AddTeamMemberRequest body)
    {
        var validator = new AddTeamMemberRequestValidator();
        var result = validator.Validate(body);
        if (!result.IsValid)
        {
            return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("team_member_add_attempt", new { userId, teamId, body.UserId });

        var cmd = new AddTeamMemberCommand
        {
            TeamId = teamId,
            Request = body,
            AddedByUserId = userId,
            RequestorRole = role
        };

        var handler = new AddTeamMemberCommandHandler(_db, _mapper);
        try
        {
            var member = await handler.Handle(cmd);
            await _audit.LogAsync("team_member_add_success", new { userId, teamId, body.UserId });
            return StatusCode(201, new { success = true, message = "Team member added successfully", data = member });
        }
        catch (CRM.Application.UserManagement.Exceptions.TeamNotFoundException)
        {
            return NotFound(new { success = false, error = "Team not found" });
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

    [HttpDelete("{teamId}/members/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RemoveMember([FromRoute] Guid teamId, [FromRoute] Guid userId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("team_member_remove_attempt", new { requestorId, teamId, userId });

        var cmd = new RemoveTeamMemberCommand
        {
            TeamId = teamId,
            UserId = userId,
            RemovedByUserId = requestorId,
            RequestorRole = role
        };

        var handler = new RemoveTeamMemberCommandHandler(_db);
        try
        {
            await handler.Handle(cmd);
            await _audit.LogAsync("team_member_remove_success", new { requestorId, teamId, userId });
            return Ok(new { success = true, message = "Team member removed successfully" });
        }
        catch (CRM.Application.UserManagement.Exceptions.TeamNotFoundException)
        {
            return NotFound(new { success = false, error = "Team not found" });
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

    [HttpGet("{teamId}/members")]
    public async Task<IActionResult> GetMembers([FromRoute] Guid teamId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        var handler = new GetTeamMembersQueryHandler(_db, _mapper);
        try
        {
            var result = await handler.Handle(new GetTeamMembersQuery
            {
                TeamId = teamId,
                RequestorUserId = requestorId,
                RequestorRole = role
            });
            return Ok(new { success = true, data = result });
        }
        catch (CRM.Application.UserManagement.Exceptions.TeamNotFoundException)
        {
            return NotFound(new { success = false, error = "Team not found" });
        }
    }
}

