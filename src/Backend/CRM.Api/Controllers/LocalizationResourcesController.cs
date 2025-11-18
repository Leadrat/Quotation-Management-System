using System;
using System.Collections.Generic;
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
public class LocalizationResourcesController : ControllerBase
{
    private readonly GetLocalizationResourcesQueryHandler _getResourcesHandler;
    private readonly CreateLocalizationResourceCommandHandler _createResourceHandler;
    private readonly UpdateLocalizationResourceCommandHandler _updateResourceHandler;
    private readonly DeleteLocalizationResourceCommandHandler _deleteResourceHandler;

    public LocalizationResourcesController(
        GetLocalizationResourcesQueryHandler getResourcesHandler,
        CreateLocalizationResourceCommandHandler createResourceHandler,
        UpdateLocalizationResourceCommandHandler updateResourceHandler,
        DeleteLocalizationResourceCommandHandler deleteResourceHandler)
    {
        _getResourcesHandler = getResourcesHandler;
        _createResourceHandler = createResourceHandler;
        _updateResourceHandler = updateResourceHandler;
        _deleteResourceHandler = deleteResourceHandler;
    }

    [HttpGet("{languageCode}")]
    [AllowAnonymous]
    public async Task<ActionResult<Dictionary<string, string>>> GetResources(string languageCode, [FromQuery] string? category)
    {
        try
        {
            var query = new GetLocalizationResourcesQuery
            {
                LanguageCode = languageCode,
                Category = category
            };
            var result = await _getResourcesHandler.Handle(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log the full exception for debugging
            var errorMessage = ex.Message;
            if (ex.InnerException != null)
            {
                errorMessage += $" | Inner: {ex.InnerException.Message}";
            }
            return StatusCode(500, new { error = errorMessage });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LocalizationResourceDto>> CreateResource([FromBody] CreateLocalizationResourceRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
        var command = new CreateLocalizationResourceCommand
        {
            Request = request,
            CreatedByUserId = userId
        };
        var result = await _createResourceHandler.Handle(command);
        return CreatedAtAction(nameof(GetResources), new { languageCode = result.LanguageCode }, result);
    }

    [HttpPut("{resourceId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LocalizationResourceDto>> UpdateResource(Guid resourceId, [FromBody] UpdateLocalizationResourceRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
        var command = new UpdateLocalizationResourceCommand
        {
            ResourceId = resourceId,
            Request = request,
            UpdatedByUserId = userId
        };
        var result = await _updateResourceHandler.Handle(command);
        return Ok(result);
    }

    [HttpDelete("{resourceId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteResource(Guid resourceId)
    {
        var command = new DeleteLocalizationResourceCommand { ResourceId = resourceId };
        await _deleteResourceHandler.Handle(command);
        return NoContent();
    }
}

