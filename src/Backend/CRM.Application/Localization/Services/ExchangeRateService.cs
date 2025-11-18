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

public class ExchangeRateService : IExchangeRateService
{
    private readonly IAppDbContext _db;
    private readonly ILogger<ExchangeRateService> _logger;

    public ExchangeRateService(IAppDbContext db, ILogger<ExchangeRateService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<ExchangeRateDto>> GetLatestRatesAsync()
    {
        var today = DateTime.UtcNow.Date;
        var rates = await _db.ExchangeRates
            .Where(r => r.IsActive &&
                       r.EffectiveDate <= today &&
                       (r.ExpiryDate == null || r.ExpiryDate >= today))
            .GroupBy(r => new { r.FromCurrencyCode, r.ToCurrencyCode })
            .Select(g => g.OrderByDescending(r => r.EffectiveDate).First())
            .ToListAsync();

        return rates.Select(r => new ExchangeRateDto
        {
            ExchangeRateId = r.ExchangeRateId,
            FromCurrencyCode = r.FromCurrencyCode,
            ToCurrencyCode = r.ToCurrencyCode,
            Rate = r.Rate,
            EffectiveDate = r.EffectiveDate,
            ExpiryDate = r.ExpiryDate,
            Source = r.Source,
            IsActive = r.IsActive
        }).ToList();
    }

    public async Task<ExchangeRateDto?> GetRateAsync(string fromCurrencyCode, string toCurrencyCode, DateTime? asOfDate = null)
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

        if (rate == null)
            return null;

        return new ExchangeRateDto
        {
            ExchangeRateId = rate.ExchangeRateId,
            FromCurrencyCode = rate.FromCurrencyCode,
            ToCurrencyCode = rate.ToCurrencyCode,
            Rate = rate.Rate,
            EffectiveDate = rate.EffectiveDate,
            ExpiryDate = rate.ExpiryDate,
            Source = rate.Source,
            IsActive = rate.IsActive
        };
    }

    public async Task<List<ExchangeRateDto>> GetHistoricalRatesAsync(string fromCurrencyCode, string toCurrencyCode, DateTime fromDate, DateTime toDate)
    {
        var rates = await _db.ExchangeRates
            .Where(r => r.FromCurrencyCode == fromCurrencyCode &&
                       r.ToCurrencyCode == toCurrencyCode &&
                       r.EffectiveDate >= fromDate &&
                       r.EffectiveDate <= toDate)
            .OrderBy(r => r.EffectiveDate)
            .ToListAsync();

        return rates.Select(r => new ExchangeRateDto
        {
            ExchangeRateId = r.ExchangeRateId,
            FromCurrencyCode = r.FromCurrencyCode,
            ToCurrencyCode = r.ToCurrencyCode,
            Rate = r.Rate,
            EffectiveDate = r.EffectiveDate,
            ExpiryDate = r.ExpiryDate,
            Source = r.Source,
            IsActive = r.IsActive
        }).ToList();
    }

    public async Task<ExchangeRateDto> UpdateExchangeRateAsync(UpdateExchangeRateRequest request)
    {
        // Deactivate existing rates for this pair
        var existingRates = await _db.ExchangeRates
            .Where(r => r.FromCurrencyCode == request.FromCurrencyCode &&
                       r.ToCurrencyCode == request.ToCurrencyCode &&
                       r.IsActive)
            .ToListAsync();

        foreach (var rate in existingRates)
        {
            rate.IsActive = false;
            rate.UpdatedAt = DateTimeOffset.UtcNow;
        }

        // Create new rate
        var newRate = new ExchangeRate
        {
            ExchangeRateId = Guid.NewGuid(),
            FromCurrencyCode = request.FromCurrencyCode,
            ToCurrencyCode = request.ToCurrencyCode,
            Rate = request.Rate,
            EffectiveDate = request.EffectiveDate,
            ExpiryDate = request.ExpiryDate,
            Source = request.Source ?? "Manual",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.ExchangeRates.Add(newRate);
        await _db.SaveChangesAsync();

        return new ExchangeRateDto
        {
            ExchangeRateId = newRate.ExchangeRateId,
            FromCurrencyCode = newRate.FromCurrencyCode,
            ToCurrencyCode = newRate.ToCurrencyCode,
            Rate = newRate.Rate,
            EffectiveDate = newRate.EffectiveDate,
            ExpiryDate = newRate.ExpiryDate,
            Source = newRate.Source,
            IsActive = newRate.IsActive
        };
    }
}


