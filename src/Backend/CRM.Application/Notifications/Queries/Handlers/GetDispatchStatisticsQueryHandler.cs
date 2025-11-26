using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Queries.Handlers;

/// <summary>
/// Handler for getting dispatch statistics
/// </summary>
public class GetDispatchStatisticsQueryHandler : IRequestHandler<GetDispatchStatisticsQuery, GetDispatchStatisticsResponse>
{
    private readonly IAppDbContext _context;
    private readonly ILogger<GetDispatchStatisticsQueryHandler> _logger;

    public GetDispatchStatisticsQueryHandler(
        IAppDbContext context,
        ILogger<GetDispatchStatisticsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetDispatchStatisticsResponse> Handle(GetDispatchStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting dispatch statistics from {FromDate} to {ToDate} for channel {Channel}",
                request.FromDate, request.ToDate, request.Channel);

            // Build the base query
            var query = _context.NotificationDispatchAttempts.AsQueryable();

            // Apply date filters
            if (request.FromDate.HasValue)
            {
                query = query.Where(da => da.AttemptedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(da => da.AttemptedAt <= request.ToDate.Value);
            }

            // Apply channel filter
            if (request.Channel.HasValue)
            {
                query = query.Where(da => da.Channel == request.Channel.Value);
            }

            var allAttempts = await query.ToListAsync(cancellationToken);

            // Calculate overall statistics
            var statistics = CalculateOverallStatistics(allAttempts);

            // Calculate daily summaries
            var dailySummaries = CalculateDailySummaries(allAttempts);

            // Calculate channel performances
            var channelPerformances = CalculateChannelPerformances(allAttempts);

            var response = new GetDispatchStatisticsResponse
            {
                Statistics = statistics,
                DailySummaries = dailySummaries,
                ChannelPerformances = channelPerformances
            };

            _logger.LogInformation("Retrieved dispatch statistics: {TotalAttempts} total attempts, {SuccessRate}% success rate",
                statistics.TotalAttempts, statistics.SuccessRate);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dispatch statistics");
            throw;
        }
    }

    private static DispatchStatistics CalculateOverallStatistics(List<Domain.Entities.NotificationDispatchAttempt> attempts)
    {
        var statistics = new DispatchStatistics
        {
            TotalAttempts = attempts.Count,
            SuccessfulDeliveries = attempts.Count(da => da.Status == DispatchStatus.Delivered),
            FailedAttempts = attempts.Count(da => da.Status == DispatchStatus.Failed),
            PendingAttempts = attempts.Count(da => da.Status == DispatchStatus.Pending),
            PermanentFailures = attempts.Count(da => da.Status == DispatchStatus.PermanentlyFailed),
            AttemptsByChannel = attempts
                .GroupBy(da => da.Channel)
                .ToDictionary(g => g.Key, g => g.Count()),
            AttemptsByStatus = attempts
                .GroupBy(da => da.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        // Calculate success rate
        if (statistics.TotalAttempts > 0)
        {
            statistics.SuccessRate = (double)statistics.SuccessfulDeliveries / statistics.TotalAttempts * 100;
        }

        // Calculate average delivery time for successful deliveries
        var successfulDeliveries = attempts
            .Where(da => da.Status == DispatchStatus.Delivered && da.DeliveredAt.HasValue)
            .ToList();

        if (successfulDeliveries.Any())
        {
            statistics.AverageDeliveryTime = successfulDeliveries
                .Average(da => (da.DeliveredAt!.Value - da.AttemptedAt).TotalMinutes);
        }

        return statistics;
    }

    private static List<DailyDispatchSummary> CalculateDailySummaries(List<Domain.Entities.NotificationDispatchAttempt> attempts)
    {
        return attempts
            .GroupBy(da => da.AttemptedAt.Date)
            .Select(g => new DailyDispatchSummary
            {
                Date = g.Key,
                TotalAttempts = g.Count(),
                SuccessfulDeliveries = g.Count(da => da.Status == DispatchStatus.Delivered),
                FailedAttempts = g.Count(da => da.Status == DispatchStatus.Failed),
                SuccessRate = g.Count() > 0 ? (double)g.Count(da => da.Status == DispatchStatus.Delivered) / g.Count() * 100 : 0,
                AverageDeliveryTime = g.Where(da => da.Status == DispatchStatus.Delivered && da.DeliveredAt.HasValue)
                    .Any() ? g.Where(da => da.Status == DispatchStatus.Delivered && da.DeliveredAt.HasValue)
                    .Average(da => (da.DeliveredAt!.Value - da.AttemptedAt).TotalMinutes) : 0
            })
            .OrderBy(s => s.Date)
            .ToList();
    }

    private static List<ChannelPerformance> CalculateChannelPerformances(List<Domain.Entities.NotificationDispatchAttempt> attempts)
    {
        return attempts
            .GroupBy(da => da.Channel)
            .Select(g => new ChannelPerformance
            {
                Channel = g.Key,
                TotalAttempts = g.Count(),
                SuccessfulDeliveries = g.Count(da => da.Status == DispatchStatus.Delivered),
                FailedAttempts = g.Count(da => da.Status == DispatchStatus.Failed),
                SuccessRate = g.Count() > 0 ? (double)g.Count(da => da.Status == DispatchStatus.Delivered) / g.Count() * 100 : 0,
                AverageDeliveryTime = g.Where(da => da.Status == DispatchStatus.Delivered && da.DeliveredAt.HasValue)
                    .Any() ? g.Where(da => da.Status == DispatchStatus.Delivered && da.DeliveredAt.HasValue)
                    .Average(da => (da.DeliveredAt!.Value - da.AttemptedAt).TotalMinutes) : 0,
                StatusBreakdown = g.GroupBy(da => da.Status)
                    .ToDictionary(sg => sg.Key, sg => sg.Count())
            })
            .OrderBy(cp => cp.Channel)
            .ToList();
    }
}