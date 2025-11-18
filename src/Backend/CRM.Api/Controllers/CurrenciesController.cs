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
public class CurrenciesController : ControllerBase
{
    private readonly GetSupportedCurrenciesQueryHandler _getCurrenciesHandler;
    private readonly CreateCurrencyCommandHandler _createCurrencyHandler;

    public CurrenciesController(
        GetSupportedCurrenciesQueryHandler getCurrenciesHandler,
        CreateCurrencyCommandHandler createCurrencyHandler)
    {
        _getCurrenciesHandler = getCurrenciesHandler;
        _createCurrencyHandler = createCurrencyHandler;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CurrencyDto>>> GetSupportedCurrencies()
    {
        try
        {
            var query = new GetSupportedCurrenciesQuery();
            var result = await _getCurrenciesHandler.Handle(query);
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
    public async Task<ActionResult<CurrencyDto>> CreateCurrency([FromBody] CreateCurrencyRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
        var command = new CreateCurrencyCommand
        {
            Request = request,
            CreatedByUserId = userId
        };
        var result = await _createCurrencyHandler.Handle(command);
        return CreatedAtAction(nameof(GetSupportedCurrencies), new { code = result.CurrencyCode }, result);
    }
}

