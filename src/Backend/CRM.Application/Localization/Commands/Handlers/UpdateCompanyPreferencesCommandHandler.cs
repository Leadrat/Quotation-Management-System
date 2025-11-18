using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;
using CRM.Application.Localization.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Commands.Handlers;

public class UpdateCompanyPreferencesCommandHandler
{
    private readonly ICompanyPreferenceService _preferenceService;
    private readonly ILogger<UpdateCompanyPreferencesCommandHandler> _logger;

    public UpdateCompanyPreferencesCommandHandler(
        ICompanyPreferenceService preferenceService,
        ILogger<UpdateCompanyPreferencesCommandHandler> logger)
    {
        _preferenceService = preferenceService;
        _logger = logger;
    }

    public async Task<CompanyPreferencesDto> Handle(UpdateCompanyPreferencesCommand command)
    {
        var result = await _preferenceService.UpdateCompanyPreferencesAsync(
            command.CompanyId,
            command.Request,
            command.UpdatedByUserId);

        _logger.LogInformation("Company preferences updated for company {CompanyId}", command.CompanyId);
        return result;
    }
}


