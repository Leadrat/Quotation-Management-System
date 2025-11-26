using CRM.Domain.Enums;
using MediatR;

namespace CRM.Application.Notifications.Queries;

/// <summary>
/// Query to get dispatch statistics for a specific time period
/// </summary>
public class GetDispatchStatisticsQuery : IRequest<GetDispatchStatisticsResponse>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public NotificationChannel? Channel { get; set; }
}

/// <summary>
/// Response containing dispatch statistics
/// </summary>
public class GetDispatchStatisticsResponse
{
    public DispatchStatistics Statistics { get; set; } = new();
    public List<DailyDispatchSummary> DailySummaries { get; set; } = new();
    public List<ChannelPerformance> ChannelPerformances { get; set; } = new();
}

/// <summary>
/// Daily dispatch summary for trend analysis
/// </summary>
public class DailyDispatchSummary
{
    public DateTime Date { get; set; }
    public int TotalAttempts { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedAttempts { get; set; }
    public double SuccessRate { get; set; }
    public double AverageDeliveryTime { get; set; }
}

/// <summary>
/// Performance metrics by channel
/// </summary>
public class ChannelPerformance
{
    public NotificationChannel Channel { get; set; }
    public int TotalAttempts { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedAttempts { get; set; }
    public double SuccessRate { get; set; }
    public double AverageDeliveryTime { get; set; }
    public Dictionary<DispatchStatus, int> StatusBreakdown { get; set; } = new();
}