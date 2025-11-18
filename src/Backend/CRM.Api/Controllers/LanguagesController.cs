using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Application.Localization.Queries;
using CRM.Application.Localization.Queries.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LanguagesController : ControllerBase
{
    private readonly GetSupportedLanguagesQueryHandler _getLanguagesHandler;

    public LanguagesController(GetSupportedLanguagesQueryHandler getLanguagesHandler)
    {
        _getLanguagesHandler = getLanguagesHandler;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CRM.Application.Localization.Dtos.SupportedLanguageDto>>> GetSupportedLanguages()
    {
        try
        {
            var query = new GetSupportedLanguagesQuery();
            var result = await _getLanguagesHandler.Handle(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Check if this is a missing table error
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
                exceptionMessage.Contains("Languages") ||
                exceptionMessage.Contains("LocalizationResources"))
            {
                // Return default languages when table doesn't exist
                return Ok(new List<CRM.Application.Localization.Dtos.SupportedLanguageDto>
                {
                    new CRM.Application.Localization.Dtos.SupportedLanguageDto { LanguageCode = "en", DisplayName = "English", DisplayNameEn = "English", NativeName = "English", IsRTL = false, IsActive = true, FlagIcon = null },
                    new CRM.Application.Localization.Dtos.SupportedLanguageDto { LanguageCode = "hi", DisplayName = "Hindi", DisplayNameEn = "Hindi", NativeName = "हिंदी", IsRTL = false, IsActive = true, FlagIcon = null }
                });
            }

            return StatusCode(500, new { error = ex.Message });
        }
    }
}

