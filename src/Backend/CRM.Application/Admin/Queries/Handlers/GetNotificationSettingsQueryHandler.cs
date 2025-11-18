using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;

namespace CRM.Application.Admin.Queries.Handlers;

public class GetNotificationSettingsQueryHandler
{
    private readonly INotificationSettingsService _settingsService;

    public GetNotificationSettingsQueryHandler(INotificationSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<NotificationSettingsDto?> Handle(GetNotificationSettingsQuery query)
    {
        return await _settingsService.GetSettingsAsync();
    }
}

