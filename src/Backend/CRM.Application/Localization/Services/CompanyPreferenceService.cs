using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Localization.Dtos;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Services;

public class CompanyPreferenceService : ICompanyPreferenceService
{
    private readonly IAppDbContext _db;
    private readonly ILogger<CompanyPreferenceService> _logger;

    public CompanyPreferenceService(IAppDbContext db, ILogger<CompanyPreferenceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CompanyPreferencesDto> GetCompanyPreferencesAsync(Guid companyId)
    {
        var preferences = await _db.CompanyPreferences.FindAsync(companyId);

        if (preferences == null)
        {
            // Return defaults
            return new CompanyPreferencesDto
            {
                CompanyId = companyId,
                DefaultLanguageCode = "en",
                DefaultCurrencyCode = "INR",
                DateFormat = "dd/MM/yyyy",
                TimeFormat = "24h",
                NumberFormat = "en-IN",
                FirstDayOfWeek = 1
            };
        }

        return new CompanyPreferencesDto
        {
            CompanyId = preferences.CompanyId,
            DefaultLanguageCode = preferences.DefaultLanguageCode,
            DefaultCurrencyCode = preferences.DefaultCurrencyCode,
            DateFormat = preferences.DateFormat,
            TimeFormat = preferences.TimeFormat,
            NumberFormat = preferences.NumberFormat,
            Timezone = preferences.Timezone,
            FirstDayOfWeek = preferences.FirstDayOfWeek
        };
    }

    public async Task<CompanyPreferencesDto> UpdateCompanyPreferencesAsync(Guid companyId, UpdateCompanyPreferencesRequest request, Guid userId)
    {
        var preferences = await _db.CompanyPreferences.FindAsync(companyId);

        if (preferences == null)
        {
            preferences = new CompanyPreferences
            {
                CompanyId = companyId,
                DefaultLanguageCode = request.DefaultLanguageCode ?? "en",
                DefaultCurrencyCode = request.DefaultCurrencyCode ?? "INR",
                DateFormat = request.DateFormat ?? "dd/MM/yyyy",
                TimeFormat = request.TimeFormat ?? "24h",
                NumberFormat = request.NumberFormat ?? "en-IN",
                Timezone = request.Timezone,
                FirstDayOfWeek = request.FirstDayOfWeek ?? 1,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedByUserId = userId
            };
            _db.CompanyPreferences.Add(preferences);
        }
        else
        {
            if (request.DefaultLanguageCode != null)
                preferences.DefaultLanguageCode = request.DefaultLanguageCode;
            if (request.DefaultCurrencyCode != null)
                preferences.DefaultCurrencyCode = request.DefaultCurrencyCode;
            if (request.DateFormat != null)
                preferences.DateFormat = request.DateFormat;
            if (request.TimeFormat != null)
                preferences.TimeFormat = request.TimeFormat;
            if (request.NumberFormat != null)
                preferences.NumberFormat = request.NumberFormat;
            if (request.Timezone != null)
                preferences.Timezone = request.Timezone;
            if (request.FirstDayOfWeek.HasValue)
                preferences.FirstDayOfWeek = request.FirstDayOfWeek.Value;

            preferences.UpdatedAt = DateTimeOffset.UtcNow;
            preferences.UpdatedByUserId = userId;
        }

        await _db.SaveChangesAsync();

        return await GetCompanyPreferencesAsync(companyId);
    }
}


