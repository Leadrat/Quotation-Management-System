using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.Application.Localization.Services;

public interface ILocalizationService
{
    Task<string> GetLocalizedStringAsync(string key, string languageCode);
    Task<Dictionary<string, string>> GetLocalizedStringsAsync(string languageCode, string? category = null);
    Task<Dictionary<string, string>> GetAllResourcesForLanguageAsync(string languageCode);
    Task<string> GetFallbackStringAsync(string key);
}


