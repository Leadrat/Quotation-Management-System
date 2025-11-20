using System.Collections.Generic;

namespace CRM.Application.UserManagement.Requests;

public class UpdateUserProfileRequest
{
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public List<string>? Skills { get; set; }
}

