using System;

namespace CRM.Application.UserManagement.Requests;

public class AssignTaskRequest
{
    public string EntityType { get; set; } = string.Empty; // Quotation, Approval, Client
    public Guid EntityId { get; set; }
    public Guid AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
}

