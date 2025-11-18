using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;

namespace CRM.Application.Admin.Queries.Handlers;

public class GetSystemSettingsQueryHandler
{
    private readonly ISystemSettingsService _settingsService;

    public GetSystemSettingsQueryHandler(ISystemSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<SystemSettingsDto> Handle(GetSystemSettingsQuery query)
    {
        var settings = await _settingsService.GetAllSettingsAsync();
        return new SystemSettingsDto { Settings = settings };
    }
}

