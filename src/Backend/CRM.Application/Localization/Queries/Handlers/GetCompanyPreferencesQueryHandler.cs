using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;
using CRM.Application.Localization.Services;

namespace CRM.Application.Localization.Queries.Handlers;

public class GetCompanyPreferencesQueryHandler
{
    private readonly ICompanyPreferenceService _preferenceService;

    public GetCompanyPreferencesQueryHandler(ICompanyPreferenceService preferenceService)
    {
        _preferenceService = preferenceService;
    }

    public async Task<CompanyPreferencesDto> Handle(GetCompanyPreferencesQuery query)
    {
        return await _preferenceService.GetCompanyPreferencesAsync(query.CompanyId);
    }
}


