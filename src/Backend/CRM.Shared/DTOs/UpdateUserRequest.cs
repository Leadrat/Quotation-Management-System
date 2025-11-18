using System;

namespace CRM.Shared.DTOs;

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Mobile { get; set; }
    public string? PhoneCode { get; set; }
}
