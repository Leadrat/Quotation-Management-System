using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Localization.Dtos;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Services;

public class CurrencyService : ICurrencyService
{
    private readonly IAppDbContext _db;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(IAppDbContext db, ILogger<CurrencyService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<CurrencyDto>> GetSupportedCurrenciesAsync()
    {
        try
        {
            var currencies = await _db.Currencies
                .Where(c => c.IsActive)
                .OrderBy(c => c.CurrencyCode)
                .ToListAsync();

            return currencies.Select(c => new CurrencyDto
            {
                CurrencyCode = c.CurrencyCode,
                DisplayName = c.DisplayName,
                Symbol = c.Symbol,
                DecimalPlaces = c.DecimalPlaces,
                IsDefault = c.IsDefault,
                IsActive = c.IsActive
            }).ToList();
        }
        catch (Exception ex)
        {
            // Check if this is a missing table error (check both outer and inner exceptions)
            var exceptionMessage = ex.Message;
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                exceptionMessage += " | " + innerException.Message;
                innerException = innerException.InnerException;
            }

            if (exceptionMessage.Contains("42P01") || 
                exceptionMessage.Contains("does not exist") || 
                (exceptionMessage.Contains("relation") && exceptionMessage.Contains("not exist")) ||
                exceptionMessage.Contains("Invalid object name") ||
                exceptionMessage.Contains("could not be found"))
            {
                _logger.LogWarning("Currencies table does not exist, returning empty list");
                return new List<CurrencyDto>();
            }

            _logger.LogError(ex, "Error getting supported currencies");
            return new List<CurrencyDto>();
        }
    }

    public async Task<CurrencyDto?> GetCurrencyByCodeAsync(string code)
    {
        var currency = await _db.Currencies
            .FirstOrDefaultAsync(c => c.CurrencyCode == code && c.IsActive);

        if (currency == null)
            return null;

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

    public async Task<CurrencyConversionResponse> ConvertCurrencyAsync(decimal amount, string fromCurrencyCode, string toCurrencyCode, DateTime? asOfDate = null)
    {
        if (fromCurrencyCode == toCurrencyCode)
        {
            return new CurrencyConversionResponse
            {
                OriginalAmount = amount,
                FromCurrencyCode = fromCurrencyCode,
                ConvertedAmount = amount,
                ToCurrencyCode = toCurrencyCode,
                ExchangeRate = 1m
            };
        }

        var rate = await GetExchangeRateAsync(fromCurrencyCode, toCurrencyCode, asOfDate);
        if (rate == null)
        {
            throw new InvalidOperationException($"Exchange rate not found for {fromCurrencyCode} to {toCurrencyCode}");
        }

        var convertedAmount = amount * rate.Value;

        return new CurrencyConversionResponse
        {
            OriginalAmount = amount,
            FromCurrencyCode = fromCurrencyCode,
            ConvertedAmount = convertedAmount,
            ToCurrencyCode = toCurrencyCode,
            ExchangeRate = rate.Value
        };
    }

    public async Task<decimal?> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode, DateTime? asOfDate = null)
    {
        var queryDate = asOfDate ?? DateTime.UtcNow.Date;

        var rate = await _db.ExchangeRates
            .Where(r => r.FromCurrencyCode == fromCurrencyCode &&
                       r.ToCurrencyCode == toCurrencyCode &&
                       r.IsActive &&
                       r.EffectiveDate <= queryDate &&
                       (r.ExpiryDate == null || r.ExpiryDate >= queryDate))
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync();

        if (rate != null)
            return rate.Rate;

        // Try reverse rate (1 / rate)
        var reverseRate = await _db.ExchangeRates
            .Where(r => r.FromCurrencyCode == toCurrencyCode &&
                       r.ToCurrencyCode == fromCurrencyCode &&
                       r.IsActive &&
                       r.EffectiveDate <= queryDate &&
                       (r.ExpiryDate == null || r.ExpiryDate >= queryDate))
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync();

        if (reverseRate != null)
            return 1m / reverseRate.Rate;

        return null;
    }
}


