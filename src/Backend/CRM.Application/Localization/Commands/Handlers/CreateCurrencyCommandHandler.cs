using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Localization.Dtos;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Commands.Handlers;

public class CreateCurrencyCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly ILogger<CreateCurrencyCommandHandler> _logger;

    public CreateCurrencyCommandHandler(IAppDbContext db, ILogger<CreateCurrencyCommandHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CurrencyDto> Handle(CreateCurrencyCommand command)
    {
        var existing = await _db.Currencies.FindAsync(command.Request.CurrencyCode);
        if (existing != null)
        {
            throw new InvalidOperationException($"Currency {command.Request.CurrencyCode} already exists");
        }

        // If setting as default, unset other defaults
        if (command.Request.IsDefault)
        {
            var currentDefault = await _db.Currencies.FirstOrDefaultAsync(c => c.IsDefault);
            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
                currentDefault.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        var currency = new Currency
        {
            CurrencyCode = command.Request.CurrencyCode,
            DisplayName = command.Request.DisplayName,
            Symbol = command.Request.Symbol,
            DecimalPlaces = command.Request.DecimalPlaces,
            IsDefault = command.Request.IsDefault,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = command.CreatedByUserId
        };

        _db.Currencies.Add(currency);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Currency created: {CurrencyCode}", command.Request.CurrencyCode);

        return new CurrencyDto
        {
            CurrencyCode = currency.CurrencyCode,
            DisplayName = currency.DisplayName,
            Symbol = currency.Symbol,
            DecimalPlaces = currency.DecimalPlaces,
            IsDefault = currency.IsDefault,
            IsActive = currency.IsActive
        };
    }
}


