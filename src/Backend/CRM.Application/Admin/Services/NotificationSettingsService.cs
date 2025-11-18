using CRM.Application.Admin.DTOs;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Services;
using CRM.Domain.Admin;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Admin.Services;

public class NotificationSettingsService : INotificationSettingsService
{
    private readonly IAppDbContext _db;
    private readonly IHtmlSanitizer _htmlSanitizer;

    public NotificationSettingsService(IAppDbContext db, IHtmlSanitizer htmlSanitizer)
    {
        _db = db;
        _htmlSanitizer = htmlSanitizer;
    }

    public async Task<NotificationSettingsDto?> GetSettingsAsync()
    {
        var settings = await _db.NotificationSettings.FirstOrDefaultAsync();
        if (settings == null) return null;

        return new NotificationSettingsDto
        {
            Id = settings.Id,
            BannerMessage = settings.BannerMessage,
            BannerType = settings.BannerType,
            IsVisible = settings.IsVisible,
            UpdatedAt = settings.UpdatedAt,
            UpdatedBy = settings.UpdatedBy
        };
    }

    public async Task<NotificationSettingsDto> UpdateSettingsAsync(
        string? bannerMessage,
        string? bannerType,
        bool isVisible,
        Guid updatedBy)
    {
        var settings = await _db.NotificationSettings.FirstOrDefaultAsync();
        var now = DateTimeOffset.UtcNow;

        if (settings == null)
        {
            settings = new NotificationSettings
            {
                Id = Guid.NewGuid(),
                BannerMessage = !string.IsNullOrEmpty(bannerMessage) ? _htmlSanitizer.Sanitize(bannerMessage) : null,
                BannerType = bannerType,
                IsVisible = isVisible,
                UpdatedAt = now,
                UpdatedBy = updatedBy
            };
            _db.NotificationSettings.Add(settings);
        }
        else
        {
            if (bannerMessage != null) settings.BannerMessage = _htmlSanitizer.Sanitize(bannerMessage);
            if (bannerType != null) settings.BannerType = bannerType;
            settings.IsVisible = isVisible;
            settings.UpdatedAt = now;
            settings.UpdatedBy = updatedBy;
        }

        await _db.SaveChangesAsync();

        return new NotificationSettingsDto
        {
            Id = settings.Id,
            BannerMessage = settings.BannerMessage,
            BannerType = settings.BannerType,
            IsVisible = settings.IsVisible,
            UpdatedAt = settings.UpdatedAt,
            UpdatedBy = settings.UpdatedBy
        };
    }
}

