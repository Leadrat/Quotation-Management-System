using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;

namespace CRM.Application.Admin.Queries.Handlers;

public class GetBrandingQueryHandler
{
    private readonly IBrandingService _brandingService;

    public GetBrandingQueryHandler(IBrandingService brandingService)
    {
        _brandingService = brandingService;
    }

    public async Task<CustomBrandingDto?> Handle(GetBrandingQuery query)
    {
        return await _brandingService.GetBrandingAsync();
    }
}

