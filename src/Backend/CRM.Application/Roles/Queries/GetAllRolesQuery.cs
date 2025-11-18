namespace CRM.Application.Roles.Queries;

public class GetAllRolesQuery
{
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
