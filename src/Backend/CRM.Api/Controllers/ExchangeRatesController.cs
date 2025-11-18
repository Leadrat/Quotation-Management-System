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
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExchangeRatesController : ControllerBase
{
    private readonly GetExchangeRatesQueryHandler _getRatesHandler;
    private readonly UpdateExchangeRateCommandHandler _updateRateHandler;

    public ExchangeRatesController(
        GetExchangeRatesQueryHandler getRatesHandler,
        UpdateExchangeRateCommandHandler updateRateHandler)
    {
        _getRatesHandler = getRatesHandler;
        _updateRateHandler = updateRateHandler;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExchangeRateDto>>> GetExchangeRates(
        [FromQuery] string? fromCurrencyCode,
        [FromQuery] string? toCurrencyCode,
        [FromQuery] DateTime? asOfDate)
    {
        var query = new GetExchangeRatesQuery
        {
            FromCurrencyCode = fromCurrencyCode,
            ToCurrencyCode = toCurrencyCode,
            AsOfDate = asOfDate
        };
        var result = await _getRatesHandler.Handle(query);
        return Ok(result);
    }

    [HttpPost("convert")]
    public async Task<ActionResult<CurrencyConversionResponse>> ConvertCurrency([FromBody] CurrencyConversionRequest request)
    {
        var query = new ConvertCurrencyQuery { Request = request };
        var handler = new ConvertCurrencyQueryHandler(
            HttpContext.RequestServices.GetRequiredService<CRM.Application.Localization.Services.ICurrencyService>(),
            HttpContext.RequestServices.GetRequiredService<CRM.Application.Localization.Services.ILocaleFormatter>());
        var result = await handler.Handle(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ExchangeRateDto>> UpdateExchangeRate([FromBody] UpdateExchangeRateRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
        var command = new UpdateExchangeRateCommand
        {
            Request = request,
            CreatedByUserId = userId
        };
        var result = await _updateRateHandler.Handle(command);
        return Ok(result);
    }
}

