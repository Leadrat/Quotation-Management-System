namespace CRM.Application.UserManagement.Queries;

public class GetCustomRolesQuery
{
    public bool? IsActive { get; set; }
    public bool? IncludeBuiltIn { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

