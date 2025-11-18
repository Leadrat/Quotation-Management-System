using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.Localization.Services;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Queries.Handlers;

public class GetLocalizationResourcesQueryHandler
{
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<GetLocalizationResourcesQueryHandler> _logger;

    public GetLocalizationResourcesQueryHandler(ILocalizationService localizationService, ILogger<GetLocalizationResourcesQueryHandler> logger)
    {
        _localizationService = localizationService;
        _logger = logger;
    }

    public async Task<Dictionary<string, string>> Handle(GetLocalizationResourcesQuery query)
    {
        try
        {
            return await _localizationService.GetLocalizedStringsAsync(query.LanguageCode, query.Category);
        }
        catch (Exception ex)
        {
            // Check if this is a missing table error (check both outer and inner exceptions)
            var exceptionMessage = ex.Message;
            var innerException = ex.InnerException;
            while (innerException != null)
            {
                exceptionMessage += " | " + innerException.Message;
                innerException = innerException.InnerException;
            }

            if (exceptionMessage.Contains("42P01") || 
                exceptionMessage.Contains("does not exist") || 
                (exceptionMessage.Contains("relation") && exceptionMessage.Contains("not exist")) ||
                exceptionMessage.Contains("Invalid object name") ||
                exceptionMessage.Contains("could not be found"))
            {
                _logger.LogWarning("LocalizationResources table does not exist, returning empty dictionary for language {LanguageCode}", query.LanguageCode);
                return new Dictionary<string, string>();
            }

            _logger.LogError(ex, "Error getting localization resources for language {LanguageCode}", query.LanguageCode);
            throw;
        }
    }
}


