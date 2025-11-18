using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Localization.Dtos;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Queries.Handlers;

public class GetSupportedLanguagesQueryHandler
{
    private readonly IAppDbContext _db;
    private readonly ILogger<GetSupportedLanguagesQueryHandler>? _logger;

    public GetSupportedLanguagesQueryHandler(IAppDbContext db, ILogger<GetSupportedLanguagesQueryHandler>? logger = null)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<SupportedLanguageDto>> Handle(GetSupportedLanguagesQuery query)
    {
        try
        {
            var languages = await _db.SupportedLanguages
                .Where(l => l.IsActive)
                .OrderBy(l => l.DisplayNameEn)
                .ToListAsync();

            return languages.Select(l => new SupportedLanguageDto
            {
                LanguageCode = l.LanguageCode,
                DisplayName = l.DisplayName,
                DisplayNameEn = l.DisplayNameEn,
                NativeName = l.NativeName,
                IsRTL = l.IsRTL,
                IsActive = l.IsActive,
                FlagIcon = l.FlagIcon
            }).ToList();
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
                exceptionMessage.Contains("could not be found") ||
                exceptionMessage.Contains("SupportedLanguages"))
            {
                _logger?.LogWarning("SupportedLanguages table does not exist, returning default languages");
                // Return default languages when table doesn't exist
                return new List<SupportedLanguageDto>
                {
                    new SupportedLanguageDto { LanguageCode = "en", DisplayName = "English", DisplayNameEn = "English", NativeName = "English", IsRTL = false, IsActive = true, FlagIcon = null },
                    new SupportedLanguageDto { LanguageCode = "hi", DisplayName = "Hindi", DisplayNameEn = "Hindi", NativeName = "हिंदी", IsRTL = false, IsActive = true, FlagIcon = null }
                };
            }

            _logger?.LogError(ex, "Error getting supported languages");
            throw;
        }
    }
}


