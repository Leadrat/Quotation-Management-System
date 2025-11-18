using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;
using CRM.Application.Localization.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Commands.Handlers;

public class UpdateUserPreferencesCommandHandler
{
    private readonly IUserPreferenceService _preferenceService;
    private readonly ILogger<UpdateUserPreferencesCommandHandler> _logger;

    public UpdateUserPreferencesCommandHandler(
        IUserPreferenceService preferenceService,
        ILogger<UpdateUserPreferencesCommandHandler> logger)
    {
        _preferenceService = preferenceService;
        _logger = logger;
    }

    public async Task<UserPreferencesDto> Handle(UpdateUserPreferencesCommand command)
    {
        var result = await _preferenceService.UpdateUserPreferencesAsync(
            command.UserId,
            command.Request);

        _logger.LogInformation("User preferences updated for user {UserId}", command.UserId);
        return result;
    }
}


