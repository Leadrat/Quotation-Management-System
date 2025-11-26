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
using CRM.Application.UserManagement.DTOs;
using CRM.Domain.Enums;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/v1/user-profiles")]
[Authorize]
public class UserProfilesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly IMapper _mapper;

    public UserProfilesController(AppDbContext db, IAuditLogger audit, IMapper mapper)
    {
        _db = db;
        _audit = audit;
        _mapper = mapper;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetProfile([FromRoute] Guid userId)
    {
        var handler = new GetUserProfileQueryHandler(_db, _mapper);
        try
        {
            var result = await handler.Handle(new GetUserProfileQuery
            {
                UserId = userId,
                RequestorUserId = userId // For now, allow viewing any profile
            });
            return Ok(new { success = true, data = result });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, error = ex.Message });
        }
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateProfile([FromRoute] Guid userId, [FromBody] UpdateUserProfileRequest body)
    {
        var validator = new UpdateUserProfileRequestValidator();
        var result = validator.Validate(body);
        if (!result.IsValid)
        {
            return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();

        await _audit.LogAsync("user_profile_update_attempt", new { requestorId, userId });

        var cmd = new UpdateUserProfileCommand
        {
            UserId = userId,
            Request = body,
            UpdatedByUserId = requestorId
        };

        var handler = new UpdateUserProfileCommandHandler(_db, _mapper);
        try
        {
            var updated = await handler.Handle(cmd);
            await _audit.LogAsync("user_profile_update_success", new { requestorId, userId });
            return Ok(new { success = true, message = "Profile updated successfully", data = updated });
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

    [HttpPut("{userId}/out-of-office")]
    public async Task<IActionResult> SetOutOfOffice([FromRoute] Guid userId, [FromBody] SetOutOfOfficeRequest body)
    {
        var validator = new SetOutOfOfficeRequestValidator();
        var result = validator.Validate(body);
        if (!result.IsValid)
        {
            return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();

        await _audit.LogAsync("user_ooo_update_attempt", new { requestorId, userId, body.IsOutOfOffice });

        var cmd = new SetOutOfOfficeCommand
        {
            UserId = userId,
            Request = body,
            UpdatedByUserId = requestorId
        };

        var handler = new SetOutOfOfficeCommandHandler(_db, _mapper);
        try
        {
            var updated = await handler.Handle(cmd);
            await _audit.LogAsync("user_ooo_update_success", new { requestorId, userId });
            return Ok(new { success = true, message = "Out-of-office status updated successfully", data = updated });
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

    [HttpPut("{userId}/presence")]
    public async Task<IActionResult> UpdatePresence([FromRoute] Guid userId, [FromBody] string status)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();

        // Authorization: Users can only update their own presence
        if (requestorId != userId)
        {
            return Forbid("Users can only update their own presence");
        }

        if (!Enum.TryParse<PresenceStatus>(status, true, out var presenceStatus))
        {
            return BadRequest(new { success = false, error = "Invalid presence status" });
        }

        var cmd = new UpdatePresenceCommand
        {
            UserId = userId,
            Status = presenceStatus
        };

        var handler = new UpdatePresenceCommandHandler(_db);
        try
        {
            await handler.Handle(cmd);
            return Ok(new { success = true, message = "Presence updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}

