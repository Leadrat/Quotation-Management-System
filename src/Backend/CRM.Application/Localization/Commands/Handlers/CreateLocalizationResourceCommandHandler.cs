using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;
using CRM.Application.Localization.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Commands.Handlers;

public class CreateLocalizationResourceCommandHandler
{
    private readonly ILocalizationResourceManager _resourceManager;
    private readonly ILogger<CreateLocalizationResourceCommandHandler> _logger;

    public CreateLocalizationResourceCommandHandler(
        ILocalizationResourceManager resourceManager,
        ILogger<CreateLocalizationResourceCommandHandler> logger)
    {
        _resourceManager = resourceManager;
        _logger = logger;
    }

    public async Task<LocalizationResourceDto> Handle(CreateLocalizationResourceCommand command)
    {
        var result = await _resourceManager.CreateResourceAsync(
            command.Request,
            command.CreatedByUserId);

        _logger.LogInformation("Localization resource created: {ResourceKey} for language {LanguageCode}",
            command.Request.ResourceKey, command.Request.LanguageCode);
        return result;
    }
}


