using System;

namespace CRM.Application.Localization.Queries;

public class GetUserPreferencesQuery
{
    public Guid UserId { get; set; }
    public bool IncludeEffective { get; set; } = false;
}


