using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.Commands.Handlers;
using CRM.Application.UserManagement.Queries;
using CRM.Application.UserManagement.Queries.Handlers;
using CRM.Application.UserManagement.Requests;
using CRM.Application.UserManagement.Validators;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/v1/bulk-user-operations")]
[Authorize(Roles = "Admin")]
public class BulkUserOperationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuditLogger _audit;

    public BulkUserOperationsController(AppDbContext db, IAuditLogger audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpPost("invite")]
    public async Task<IActionResult> BulkInvite([FromBody] BulkInviteUsersRequest body)
    {
        var validator = new BulkInviteUsersRequestValidator();
        var result = validator.Validate(body);
        if (!result.IsValid)
        {
            return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("bulk_invite_users_attempt", new { userId, userCount = body.Users.Count });

        var cmd = new BulkInviteUsersCommand
        {
            Request = body,
            InvitedByUserId = userId,
            RequestorRole = role
        };

        var handler = new BulkInviteUsersCommandHandler(_db, new CRM.Infrastructure.Security.BCryptPasswordHasher());
        try
        {
            var operationResult = await handler.Handle(cmd);
            await _audit.LogAsync("bulk_invite_users_success", new { userId, successCount = operationResult.SuccessCount, failureCount = operationResult.FailureCount });
            return Ok(new { success = true, message = "Bulk invite completed", data = operationResult });
        }
        catch (CRM.Application.UserManagement.Exceptions.UnauthorizedTeamOperationException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateUsersRequest body)
    {
        var validator = new BulkUpdateUsersRequestValidator();
        var result = validator.Validate(body);
        if (!result.IsValid)
        {
            return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("bulk_update_users_attempt", new { userId, userCount = body.UserIds.Count });

        var cmd = new BulkUpdateUsersCommand
        {
            Request = body,
            UpdatedByUserId = userId,
            RequestorRole = role
        };

        var handler = new BulkUpdateUsersCommandHandler(_db);
        try
        {
            var operationResult = await handler.Handle(cmd);
            await _audit.LogAsync("bulk_update_users_success", new { userId, successCount = operationResult.SuccessCount, failureCount = operationResult.FailureCount });
            return Ok(new { success = true, message = "Bulk update completed", data = operationResult });
        }
        catch (CRM.Application.UserManagement.Exceptions.UnauthorizedTeamOperationException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> BulkDeactivate([FromBody] Guid[] userIds)
    {
        if (userIds == null || userIds.Length == 0)
        {
            return BadRequest(new { success = false, error = "At least one user ID must be provided" });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("bulk_deactivate_users_attempt", new { requestorId, userCount = userIds.Length });

        var cmd = new BulkDeactivateUsersCommand
        {
            UserIds = userIds.ToList<Guid>(),
            DeactivatedByUserId = requestorId,
            RequestorRole = role
        };

        var handler = new BulkDeactivateUsersCommandHandler(_db);
        try
        {
            var operationResult = await handler.Handle(cmd);
            await _audit.LogAsync("bulk_deactivate_users_success", new { requestorId, successCount = operationResult.SuccessCount, failureCount = operationResult.FailureCount });
            return Ok(new { success = true, message = "Bulk deactivation completed", data = operationResult });
        }
        catch (CRM.Application.UserManagement.Exceptions.UnauthorizedTeamOperationException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportUsers(
        [FromQuery] string format = "CSV",
        [FromQuery] Guid? roleId = null,
        [FromQuery] Guid? teamId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        var handler = new ExportUsersQueryHandler(_db);
        try
        {
            var exportData = await handler.Handle(new ExportUsersQuery
            {
                Format = format,
                RoleId = roleId,
                TeamId = teamId,
                IsActive = isActive,
                CreatedFrom = createdFrom,
                CreatedTo = createdTo,
                RequestorUserId = requestorId,
                RequestorRole = role
            });

            var contentType = format.ToUpperInvariant() switch
            {
                "CSV" => "text/csv",
                "EXCEL" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "JSON" => "application/json",
                _ => "text/csv"
            };

            var fileName = $"users_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format.ToLowerInvariant()}";
            return File(exportData, contentType, fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}

