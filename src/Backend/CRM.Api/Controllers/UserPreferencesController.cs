using System;
using System.Threading.Tasks;
using CRM.Application.Localization.Commands;
using CRM.Application.Localization.Commands.Handlers;
using CRM.Application.Localization.Dtos;
using CRM.Application.Localization.Queries;
using CRM.Application.Localization.Queries.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserPreferencesController : ControllerBase
{
    private readonly GetUserPreferencesQueryHandler _getPreferencesHandler;
    private readonly UpdateUserPreferencesCommandHandler _updatePreferencesHandler;

    public UserPreferencesController(
        GetUserPreferencesQueryHandler getPreferencesHandler,
        UpdateUserPreferencesCommandHandler updatePreferencesHandler)
    {
        _getPreferencesHandler = getPreferencesHandler;
        _updatePreferencesHandler = updatePreferencesHandler;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserPreferencesDto>> GetMyPreferences([FromQuery] bool includeEffective = false)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var query = new GetUserPreferencesQuery
            {
                UserId = userId,
                IncludeEffective = includeEffective
            };
            var result = await _getPreferencesHandler.Handle(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserPreferencesDto>> UpdateMyPreferences([FromBody] UpdateUserPreferencesRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var command = new UpdateUserPreferencesCommand
            {
                UserId = userId,
                Request = request
            };
            var result = await _updatePreferencesHandler.Handle(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<UserPreferencesDto>> GetUserPreferences(Guid userId, [FromQuery] bool includeEffective = false)
    {
        var query = new GetUserPreferencesQuery
        {
            UserId = userId,
            IncludeEffective = includeEffective
        };
        var result = await _getPreferencesHandler.Handle(query);
        return Ok(result);
    }
}

