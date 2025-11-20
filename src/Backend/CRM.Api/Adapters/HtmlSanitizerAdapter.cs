using CRM.Application.Common.Services;
using CRM.Infrastructure.Admin.HtmlSanitization;

namespace CRM.Api.Adapters;

public class HtmlSanitizerAdapter : CRM.Application.Common.Services.IHtmlSanitizer
{
    private readonly Infrastructure.Admin.HtmlSanitization.IHtmlSanitizer _infrastructureService;

    public HtmlSanitizerAdapter(Infrastructure.Admin.HtmlSanitization.IHtmlSanitizer infrastructureService)
    {
        _infrastructureService = infrastructureService;
    }

    public string Sanitize(string html) => _infrastructureService.Sanitize(html);
}

