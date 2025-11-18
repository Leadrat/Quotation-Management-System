using System;
using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Services;

public interface ICompanyPreferenceService
{
    Task<CompanyPreferencesDto> GetCompanyPreferencesAsync(Guid companyId);
    Task<CompanyPreferencesDto> UpdateCompanyPreferencesAsync(Guid companyId, UpdateCompanyPreferencesRequest request, Guid userId);
}


