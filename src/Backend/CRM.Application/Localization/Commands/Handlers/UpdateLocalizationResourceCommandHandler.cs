using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;
using CRM.Application.Localization.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Commands.Handlers;

public class UpdateLocalizationResourceCommandHandler
{
    private readonly ILocalizationResourceManager _resourceManager;
    private readonly ILogger<UpdateLocalizationResourceCommandHandler> _logger;

    public UpdateLocalizationResourceCommandHandler(
        ILocalizationResourceManager resourceManager,
        ILogger<UpdateLocalizationResourceCommandHandler> logger)
    {
        _resourceManager = resourceManager;
        _logger = logger;
    }

    public async Task<LocalizationResourceDto> Handle(UpdateLocalizationResourceCommand command)
    {
        var result = await _resourceManager.UpdateResourceAsync(
            command.ResourceId,
            command.Request,
            command.UpdatedByUserId);

        _logger.LogInformation("Localization resource updated: {ResourceId}", command.ResourceId);
        return result;
    }
}


