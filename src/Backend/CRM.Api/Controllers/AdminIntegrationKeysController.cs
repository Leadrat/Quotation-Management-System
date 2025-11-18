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
[Route("api/v1/admin/integrations")]
[Authorize(Roles = "Admin")]
public class AdminIntegrationKeysController : ControllerBase
{
    private readonly GetIntegrationKeysQueryHandler _getAllHandler;
    private readonly GetIntegrationKeyByIdQueryHandler _getByIdHandler;
    private readonly GetIntegrationKeyWithValueQueryHandler _getWithValueHandler;
    private readonly CreateIntegrationKeyCommandHandler _createHandler;
    private readonly UpdateIntegrationKeyCommandHandler _updateHandler;
    private readonly DeleteIntegrationKeyCommandHandler _deleteHandler;

    public AdminIntegrationKeysController(
        GetIntegrationKeysQueryHandler getAllHandler,
        GetIntegrationKeyByIdQueryHandler getByIdHandler,
        GetIntegrationKeyWithValueQueryHandler getWithValueHandler,
        CreateIntegrationKeyCommandHandler createHandler,
        UpdateIntegrationKeyCommandHandler updateHandler,
        DeleteIntegrationKeyCommandHandler deleteHandler)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _getWithValueHandler = getWithValueHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllKeys()
    {
        var query = new GetIntegrationKeysQuery();
        var result = await _getAllHandler.Handle(query);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetKeyById(Guid id)
    {
        var query = new GetIntegrationKeyByIdQuery { Id = id };
        var result = await _getByIdHandler.Handle(query);
        
        if (result == null)
        {
            return NotFound(new { success = false, message = "Integration key not found" });
        }

        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}/show")]
    public async Task<IActionResult> GetKeyWithValue(Guid id)
    {
        var query = new GetIntegrationKeyWithValueQuery { Id = id };
        var result = await _getWithValueHandler.Handle(query);
        
        if (result == null)
        {
            return NotFound(new { success = false, message = "Integration key not found" });
        }

        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> CreateKey([FromBody] CreateIntegrationKeyRequest request)
    {
        // Validate request
        var validator = new CreateIntegrationKeyRequestValidator();
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

        var command = new CreateIntegrationKeyCommand
        {
            KeyName = request.KeyName,
            KeyValue = request.KeyValue,
            Provider = request.Provider,
            CreatedBy = userId,
            IpAddress = ipAddress
        };

        var result = await _createHandler.Handle(command);
        return Ok(new { success = true, message = "Integration key created successfully", data = result });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateKey(Guid id, [FromBody] UpdateIntegrationKeyRequest request)
    {
        // Validate request
        var validator = new UpdateIntegrationKeyRequestValidator();
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

        var command = new UpdateIntegrationKeyCommand
        {
            Id = id,
            KeyName = request.KeyName,
            KeyValue = request.KeyValue,
            Provider = request.Provider,
            UpdatedBy = userId,
            IpAddress = ipAddress
        };

        try
        {
            var result = await _updateHandler.Handle(command);
            return Ok(new { success = true, message = "Integration key updated successfully", data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Integration key not found" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteKey(Guid id)
    {
        // Get user ID from JWT
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid user token" });
        }

        // Get IP address
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var command = new DeleteIntegrationKeyCommand 
        { 
            Id = id,
            DeletedBy = userId,
            IpAddress = ipAddress
        };
        var deleted = await _deleteHandler.Handle(command);

        if (!deleted)
        {
            return NotFound(new { success = false, message = "Integration key not found" });
        }

        return Ok(new { success = true, message = "Integration key deleted successfully" });
    }
}

