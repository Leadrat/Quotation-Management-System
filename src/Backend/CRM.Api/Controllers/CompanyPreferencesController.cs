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
[Authorize(Roles = "Admin")]
public class CompanyPreferencesController : ControllerBase
{
    private readonly GetCompanyPreferencesQueryHandler _getPreferencesHandler;
    private readonly UpdateCompanyPreferencesCommandHandler _updatePreferencesHandler;

    public CompanyPreferencesController(
        GetCompanyPreferencesQueryHandler getPreferencesHandler,
        UpdateCompanyPreferencesCommandHandler updatePreferencesHandler)
    {
        _getPreferencesHandler = getPreferencesHandler;
        _updatePreferencesHandler = updatePreferencesHandler;
    }

    [HttpGet("{companyId}")]
    public async Task<ActionResult<CompanyPreferencesDto>> GetCompanyPreferences(Guid companyId)
    {
        var query = new GetCompanyPreferencesQuery { CompanyId = companyId };
        var result = await _getPreferencesHandler.Handle(query);
        return Ok(result);
    }

    [HttpPut("{companyId}")]
    public async Task<ActionResult<CompanyPreferencesDto>> UpdateCompanyPreferences(
        Guid companyId,
        [FromBody] UpdateCompanyPreferencesRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
        var command = new UpdateCompanyPreferencesCommand
        {
            CompanyId = companyId,
            Request = request,
            UpdatedByUserId = userId
        };
        var result = await _updatePreferencesHandler.Handle(command);
        return Ok(result);
    }
}

