using CRM.Domain.Enums;
using MediatR;

namespace CRM.Application.Notifications.Queries;

/// <summary>
/// Query to get dispatch history for notifications
/// </summary>
public class GetDispatchHistoryQuery : IRequest<GetDispatchHistoryResponse>
{
    public Guid? NotificationId { get; set; }
    public Guid? UserId { get; set; }
    public NotificationChannel? Channel { get; set; }
    public DispatchStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Response containing dispatch history data
/// </summary>
public class GetDispatchHistoryResponse
{
    public List<DispatchHistoryItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public DispatchStatistics Statistics { get; set; } = new();
}

/// <summary>
/// Individual dispatch history item
/// </summary>
public class DispatchHistoryItem
{
    public int Id { get; set; }
    public Guid NotificationId { get; set; }
    public string NotificationTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public DispatchStatus Status { get; set; }
    public DateTime AttemptedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public int AttemptNumber { get; set; }
    public string? ExternalId { get; set; }
    public string? ErrorMessage { get; set; }
    public NotificationPriority Priority { get; set; }
}

/// <summary>
/// Dispatch statistics summary
/// </summary>
public class DispatchStatistics
{
    public int TotalAttempts { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedAttempts { get; set; }
    public int PendingAttempts { get; set; }
    public int PermanentFailures { get; set; }
    public double SuccessRate { get; set; }
    public Dictionary<NotificationChannel, int> AttemptsByChannel { get; set; } = new();
    public Dictionary<DispatchStatus, int> AttemptsByStatus { get; set; } = new();
    public double AverageDeliveryTime { get; set; } // In minutes
}