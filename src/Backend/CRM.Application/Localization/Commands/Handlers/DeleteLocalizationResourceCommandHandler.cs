using System.Threading.Tasks;
using CRM.Application.Localization.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Commands.Handlers;

public class DeleteLocalizationResourceCommandHandler
{
    private readonly ILocalizationResourceManager _resourceManager;
    private readonly ILogger<DeleteLocalizationResourceCommandHandler> _logger;

    public DeleteLocalizationResourceCommandHandler(
        ILocalizationResourceManager resourceManager,
        ILogger<DeleteLocalizationResourceCommandHandler> logger)
    {
        _resourceManager = resourceManager;
        _logger = logger;
    }

    public async Task Handle(DeleteLocalizationResourceCommand command)
    {
        await _resourceManager.DeleteResourceAsync(command.ResourceId);
        _logger.LogInformation("Localization resource deleted: {ResourceId}", command.ResourceId);
    }
}


