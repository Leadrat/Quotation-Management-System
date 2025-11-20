namespace CRM.Application.UserManagement.Requests;

public class UpdateTaskStatusRequest
{
    public string Status { get; set; } = string.Empty; // Pending, InProgress, Completed, Cancelled
}

