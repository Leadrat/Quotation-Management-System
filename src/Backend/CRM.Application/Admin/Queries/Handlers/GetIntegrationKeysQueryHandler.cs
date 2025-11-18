using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;

namespace CRM.Application.Admin.Queries.Handlers;

public class GetIntegrationKeysQueryHandler
{
    private readonly IIntegrationKeyService _keyService;

    public GetIntegrationKeysQueryHandler(IIntegrationKeyService keyService)
    {
        _keyService = keyService;
    }

    public async Task<List<IntegrationKeyDto>> Handle(GetIntegrationKeysQuery query)
    {
        return await _keyService.GetAllKeysAsync();
    }
}

