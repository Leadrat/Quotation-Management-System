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
[Route("api/v1/admin/notification-settings")]
[Authorize(Roles = "Admin")]
public class AdminNotificationSettingsController : ControllerBase
{
    private readonly GetNotificationSettingsQueryHandler _getHandler;
    private readonly UpdateNotificationSettingsCommandHandler _updateHandler;

    public AdminNotificationSettingsController(
        GetNotificationSettingsQueryHandler getHandler,
        UpdateNotificationSettingsCommandHandler updateHandler)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var query = new GetNotificationSettingsQuery();
        var result = await _getHandler.Handle(query);
        
        if (result == null)
        {
            return Ok(new { success = true, data = (object?)null });
        }

        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateNotificationSettingsRequest request)
    {
        // Validate request
        var validator = new UpdateNotificationSettingsRequestValidator();
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

        var command = new UpdateNotificationSettingsCommand
        {
            BannerMessage = request.BannerMessage,
            BannerType = request.BannerType,
            IsVisible = request.IsVisible,
            UpdatedBy = userId,
            IpAddress = ipAddress
        };

        var result = await _updateHandler.Handle(command);
        return Ok(new { success = true, message = "Notification settings updated successfully", data = result });
    }
}

