using System;

namespace CRM.Application.UserManagement.Requests;

public class SetOutOfOfficeRequest
{
    public bool IsOutOfOffice { get; set; }
    public string? Message { get; set; }
    public Guid? DelegateUserId { get; set; }
}

