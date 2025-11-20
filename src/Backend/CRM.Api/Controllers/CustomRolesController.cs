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
[Route("api/v1/custom-roles")]
[Authorize(Roles = "Admin")]
public class CustomRolesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly IMapper _mapper;

    public CustomRolesController(AppDbContext db, IAuditLogger audit, IMapper mapper)
    {
        _db = db;
        _audit = audit;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomRoleRequest body)
    {
        var validator = new CreateCustomRoleRequestValidator();
        var result = validator.Validate(body);
        if (!result.IsValid)
        {
            return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("custom_role_create_attempt", new { userId, body.RoleName });

        var cmd = new CreateCustomRoleCommand
        {
            Request = body,
            CreatedByUserId = userId,
            RequestorRole = role
        };

        var handler = new CreateCustomRoleCommandHandler(_db, _mapper);
        try
        {
            var created = await handler.Handle(cmd);
            await _audit.LogAsync("custom_role_create_success", new { userId, created.RoleId });
            return StatusCode(201, new { success = true, message = "Custom role created successfully", data = created });
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

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null, [FromQuery] bool? includeBuiltIn = false)
    {
        var handler = new GetCustomRolesQueryHandler(_db, _mapper);
        var result = await handler.Handle(new GetCustomRolesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IsActive = isActive,
            IncludeBuiltIn = includeBuiltIn
        });
        return Ok(result);
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetAvailablePermissions()
    {
        var handler = new GetAvailablePermissionsQueryHandler();
        var permissions = await handler.Handle(new GetAvailablePermissionsQuery());
        return Ok(new { success = true, data = permissions });
    }

    [HttpPut("{roleId}/permissions")]
    public async Task<IActionResult> UpdatePermissions([FromRoute] Guid roleId, [FromBody] UpdateRolePermissionsRequest body)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        await _audit.LogAsync("role_permissions_update_attempt", new { userId, roleId });

        var cmd = new UpdateRolePermissionsCommand
        {
            RoleId = roleId,
            Request = body,
            UpdatedByUserId = userId,
            RequestorRole = role
        };

        var handler = new UpdateRolePermissionsCommandHandler(_db, _mapper);
        try
        {
            var updated = await handler.Handle(cmd);
            await _audit.LogAsync("role_permissions_update_success", new { userId, roleId });
            return Ok(new { success = true, message = "Role permissions updated successfully", data = updated });
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

