using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Localization.Dtos;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Services;

public class UserPreferenceService : IUserPreferenceService
{
    private readonly IAppDbContext _db;
    private readonly ILogger<UserPreferenceService> _logger;

    public UserPreferenceService(IAppDbContext db, ILogger<UserPreferenceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<UserPreferencesDto> GetUserPreferencesAsync(Guid userId)
    {
        try
        {
            var preferences = await _db.UserPreferences.FindAsync(userId);
            
            if (preferences == null)
            {
                // Return defaults
                return new UserPreferencesDto
                {
                    UserId = userId,
                    LanguageCode = "en",
                    DateFormat = "dd/MM/yyyy",
                    TimeFormat = "24h",
                    NumberFormat = "en-IN",
                    FirstDayOfWeek = 1
                };
            }

            return new UserPreferencesDto
            {
                UserId = preferences.UserId,
                LanguageCode = preferences.LanguageCode,
                CurrencyCode = preferences.CurrencyCode,
                DateFormat = preferences.DateFormat,
                TimeFormat = preferences.TimeFormat,
                NumberFormat = preferences.NumberFormat,
                Timezone = preferences.Timezone,
                FirstDayOfWeek = preferences.FirstDayOfWeek
            };
        }
        catch (Exception ex) when (ex.Message.Contains("42P01") || ex.Message.Contains("does not exist") || ex.Message.Contains("relation") && ex.Message.Contains("not exist"))
        {
            _logger.LogWarning("UserPreferences table does not exist, returning defaults for user {UserId}", userId);
            return new UserPreferencesDto
            {
                UserId = userId,
                LanguageCode = "en",
                DateFormat = "dd/MM/yyyy",
                TimeFormat = "24h",
                NumberFormat = "en-IN",
                FirstDayOfWeek = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user preferences for user {UserId}", userId);
            // Return defaults on error
            return new UserPreferencesDto
            {
                UserId = userId,
                LanguageCode = "en",
                DateFormat = "dd/MM/yyyy",
                TimeFormat = "24h",
                NumberFormat = "en-IN",
                FirstDayOfWeek = 1
            };
        }
    }

    public async Task<UserPreferencesDto> UpdateUserPreferencesAsync(Guid userId, UpdateUserPreferencesRequest request)
    {
        try
        {
            var preferences = await _db.UserPreferences.FindAsync(userId);

            if (preferences == null)
            {
                preferences = new UserPreferences
                {
                    UserId = userId,
                    LanguageCode = request.LanguageCode ?? "en",
                    CurrencyCode = request.CurrencyCode,
                    DateFormat = request.DateFormat ?? "dd/MM/yyyy",
                    TimeFormat = request.TimeFormat ?? "24h",
                    NumberFormat = request.NumberFormat ?? "en-IN",
                    Timezone = request.Timezone,
                    FirstDayOfWeek = request.FirstDayOfWeek ?? 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _db.UserPreferences.Add(preferences);
            }
            else
            {
                if (request.LanguageCode != null)
                    preferences.LanguageCode = request.LanguageCode;
                if (request.CurrencyCode != null)
                    preferences.CurrencyCode = request.CurrencyCode;
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
            }

            await _db.SaveChangesAsync();

            return await GetUserPreferencesAsync(userId);
        }
        catch (Exception ex) when (ex.Message.Contains("42P01") || ex.Message.Contains("does not exist") || ex.Message.Contains("relation") && ex.Message.Contains("not exist"))
        {
            _logger.LogWarning("UserPreferences table does not exist, cannot update preferences for user {UserId}", userId);
            // Return defaults
            return new UserPreferencesDto
            {
                UserId = userId,
                LanguageCode = request.LanguageCode ?? "en",
                CurrencyCode = request.CurrencyCode ?? "INR",
                DateFormat = request.DateFormat ?? "dd/MM/yyyy",
                TimeFormat = request.TimeFormat ?? "24h",
                NumberFormat = request.NumberFormat ?? "en-IN",
                Timezone = request.Timezone ?? "Asia/Kolkata",
                FirstDayOfWeek = request.FirstDayOfWeek ?? 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserPreferencesDto> GetEffectivePreferencesAsync(Guid userId)
    {
        try
        {
            var userPrefs = await GetUserPreferencesAsync(userId);
            
            // Get user's company
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return userPrefs;

            // Get company preferences (assuming user has CompanyId - adjust based on your schema)
            // For now, return user preferences. Company preferences can be added when Company entity relationship is clear.
            
            // If user doesn't have currency set, use company default
            if (string.IsNullOrEmpty(userPrefs.CurrencyCode))
            {
                // TODO: Get company default currency when Company relationship is available
                // For now, use INR as default
                userPrefs.CurrencyCode = "INR";
            }

            return userPrefs;
        }
        catch (Exception ex) when (ex.Message.Contains("42P01") || ex.Message.Contains("does not exist") || ex.Message.Contains("relation") && ex.Message.Contains("not exist"))
        {
            _logger.LogWarning("Database tables do not exist, returning defaults for user {UserId}", userId);
            return new UserPreferencesDto
            {
                UserId = userId,
                LanguageCode = "en",
                DateFormat = "dd/MM/yyyy",
                TimeFormat = "24h",
                NumberFormat = "en-IN",
                FirstDayOfWeek = 1,
                CurrencyCode = "INR"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting effective preferences for user {UserId}", userId);
            return new UserPreferencesDto
            {
                UserId = userId,
                LanguageCode = "en",
                DateFormat = "dd/MM/yyyy",
                TimeFormat = "24h",
                NumberFormat = "en-IN",
                FirstDayOfWeek = 1,
                CurrencyCode = "INR"
            };
        }
    }
}


