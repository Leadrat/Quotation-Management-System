using System;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Domain.Entities;

namespace CRM.Domain.UserManagement;

[Table("TaskAssignments")]
public class TaskAssignment
{
    public Guid AssignmentId { get; set; }
    public string EntityType { get; set; } = string.Empty; // Quotation, Approval, Client
    public Guid EntityId { get; set; }
    public Guid AssignedToUserId { get; set; }
    public Guid AssignedByUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskAssignmentStatus Status { get; set; } = TaskAssignmentStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual User AssignedToUser { get; set; } = null!;
    public virtual User AssignedByUser { get; set; } = null!;

    public void MarkAsCompleted()
    {
        Status = TaskAssignmentStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsInProgress()
    {
        Status = TaskAssignmentStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = TaskAssignmentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOverdue() => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != TaskAssignmentStatus.Completed;
}

public enum TaskAssignmentStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

