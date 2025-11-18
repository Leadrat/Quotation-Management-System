using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;
using CRM.Application.Localization.Services;

namespace CRM.Application.Localization.Queries.Handlers;

public class GetUserPreferencesQueryHandler
{
    private readonly IUserPreferenceService _preferenceService;

    public GetUserPreferencesQueryHandler(IUserPreferenceService preferenceService)
    {
        _preferenceService = preferenceService;
    }

    public async Task<UserPreferencesDto> Handle(GetUserPreferencesQuery query)
    {
        if (query.IncludeEffective)
        {
            return await _preferenceService.GetEffectivePreferencesAsync(query.UserId);
        }
        return await _preferenceService.GetUserPreferencesAsync(query.UserId);
    }
}


