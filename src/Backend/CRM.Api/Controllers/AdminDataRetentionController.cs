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
[Route("api/v1/admin/data-retention")]
[Authorize(Roles = "Admin")]
public class AdminDataRetentionController : ControllerBase
{
    private readonly GetDataRetentionPoliciesQueryHandler _getHandler;
    private readonly UpdateDataRetentionPolicyCommandHandler _updateHandler;

    public AdminDataRetentionController(
        GetDataRetentionPoliciesQueryHandler getHandler,
        UpdateDataRetentionPolicyCommandHandler updateHandler)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetPolicies()
    {
        var query = new GetDataRetentionPoliciesQuery();
        var result = await _getHandler.Handle(query);
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePolicy([FromBody] UpdateDataRetentionPolicyRequest request)
    {
        // Validate request
        var validator = new UpdateDataRetentionPolicyRequestValidator();
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

        var command = new UpdateDataRetentionPolicyCommand
        {
            EntityType = request.EntityType,
            RetentionPeriodMonths = request.RetentionPeriodMonths,
            IsActive = request.IsActive,
            AutoPurgeEnabled = request.AutoPurgeEnabled,
            UpdatedBy = userId,
            IpAddress = ipAddress
        };

        var result = await _updateHandler.Handle(command);
        return Ok(new { success = true, message = "Data retention policy updated successfully", data = result });
    }
}

