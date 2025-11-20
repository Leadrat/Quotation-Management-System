using System;

namespace CRM.Application.UserManagement.Queries;

public class ExportUsersQuery
{
    public Guid? RoleId { get; set; }
    public Guid? TeamId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public string Format { get; set; } = "CSV"; // CSV, Excel, JSON
    public Guid RequestorUserId { get; set; }
    public string RequestorRole { get; set; } = string.Empty;
}

