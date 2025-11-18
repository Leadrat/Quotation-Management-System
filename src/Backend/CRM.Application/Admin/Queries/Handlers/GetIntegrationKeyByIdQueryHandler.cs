using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;

namespace CRM.Application.Admin.Queries.Handlers;

public class GetIntegrationKeyByIdQueryHandler
{
    private readonly IIntegrationKeyService _keyService;

    public GetIntegrationKeyByIdQueryHandler(IIntegrationKeyService keyService)
    {
        _keyService = keyService;
    }

    public async Task<IntegrationKeyDto?> Handle(GetIntegrationKeyByIdQuery query)
    {
        return await _keyService.GetKeyByIdAsync(query.Id);
    }
}

