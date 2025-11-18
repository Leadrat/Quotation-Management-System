using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.Localization.Dtos;

namespace CRM.Application.Localization.Services;

public interface ILocalizationResourceManager
{
    Task<LocalizationResourceDto> CreateResourceAsync(CreateLocalizationResourceRequest request, Guid userId);
    Task<LocalizationResourceDto> UpdateResourceAsync(Guid resourceId, UpdateLocalizationResourceRequest request, Guid userId);
    Task DeleteResourceAsync(Guid resourceId);
    Task ImportResourcesAsync(string languageCode, Dictionary<string, string> resources, Guid userId);
    Task<Dictionary<string, string>> ExportResourcesAsync(string languageCode);
}


