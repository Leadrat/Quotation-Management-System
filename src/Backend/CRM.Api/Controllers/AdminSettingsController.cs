using System.Security.Claims;
using CRM.Application.Admin.Commands;
using CRM.Application.Admin.Commands.Handlers;
using CRM.Application.Admin.Queries;
using CRM.Application.Admin.Queries.Handlers;
using CRM.Application.Admin.Requests;
using CRM.Application.Admin.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/v1/admin/settings")]
[Authorize(Roles = "Admin")]
public class AdminSettingsController : ControllerBase
{
    private readonly GetSystemSettingsQueryHandler _getHandler;
    private readonly UpdateSystemSettingsCommandHandler _updateHandler;

    public AdminSettingsController(
        GetSystemSettingsQueryHandler getHandler,
        UpdateSystemSettingsCommandHandler updateHandler)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var query = new GetSystemSettingsQuery();
        var result = await _getHandler.Handle(query);
        return Ok(new { success = true, data = result.Settings });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSystemSettingsRequest request)
    {
        // Validate request
        var validator = new UpdateSystemSettingsRequestValidator();
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { success = false, errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        // Get user ID from JWT
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid user token" });
        }

        // Get IP address
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        // Create command
        var command = new UpdateSystemSettingsCommand
        {
            Settings = request.Settings,
            ModifiedBy = userId,
            IpAddress = ipAddress
        };

        // Execute handler
        var result = await _updateHandler.Handle(command);

        return Ok(new { success = true, message = "Settings updated successfully", data = result.Settings });
    }
}

