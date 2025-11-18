using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;

namespace CRM.Application.Admin.Queries.Handlers;

public class GetIntegrationKeyWithValueQueryHandler
{
    private readonly IIntegrationKeyService _keyService;

    public GetIntegrationKeyWithValueQueryHandler(IIntegrationKeyService keyService)
    {
        _keyService = keyService;
    }

    public async Task<IntegrationKeyWithValueDto?> Handle(GetIntegrationKeyWithValueQuery query)
    {
        return await _keyService.GetKeyWithValueAsync(query.Id);
    }
}

