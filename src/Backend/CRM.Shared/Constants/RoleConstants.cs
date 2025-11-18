using System;

namespace CRM.Shared.Constants;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string SalesRep = "SalesRep";
    public const string Client = "Client";

    public static readonly Guid AdminRoleId = Guid.Parse("AA668EE7-79E9-4AF3-B3ED-1A47F104B8EA");
    public static readonly Guid ManagerRoleId = Guid.Parse("8D38F43B-EB54-4E4A-9582-1C611F7B5DF6");
    public static readonly Guid SalesRepRoleId = Guid.Parse("FAE6CEDB-42FD-497B-85F6-F2B14ECA0079");
    public static readonly Guid ClientRoleId = Guid.Parse("00F3CF90-C1A2-4B46-96A2-6A58EF54E8DD");

    public static string GetName(Guid roleId) => roleId switch
    {
        var id when id == AdminRoleId => Admin,
        var id when id == ManagerRoleId => Manager,
        var id when id == SalesRepRoleId => SalesRep,
        var id when id == ClientRoleId => Client,
        _ => ""
    };
}
