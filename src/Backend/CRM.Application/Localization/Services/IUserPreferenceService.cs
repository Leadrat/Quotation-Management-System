using System;
using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Services;

public interface IUserPreferenceService
{
    Task<UserPreferencesDto> GetUserPreferencesAsync(Guid userId);
    Task<UserPreferencesDto> UpdateUserPreferencesAsync(Guid userId, UpdateUserPreferencesRequest request);
    Task<UserPreferencesDto> GetEffectivePreferencesAsync(Guid userId);
}


