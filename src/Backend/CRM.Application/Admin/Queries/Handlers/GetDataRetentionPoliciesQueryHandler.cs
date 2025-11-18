using CRM.Application.Admin.DTOs;
using CRM.Application.Admin.Services;

namespace CRM.Application.Admin.Queries.Handlers;

public class GetDataRetentionPoliciesQueryHandler
{
    private readonly IDataRetentionService _retentionService;

    public GetDataRetentionPoliciesQueryHandler(IDataRetentionService retentionService)
    {
        _retentionService = retentionService;
    }

    public async Task<List<DataRetentionPolicyDto>> Handle(GetDataRetentionPoliciesQuery query)
    {
        return await _retentionService.GetAllPoliciesAsync();
    }
}

