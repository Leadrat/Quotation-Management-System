using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Notifications.Queries.Handlers;

/// <summary>
/// Handler for getting notification dispatch history
/// </summary>
public class GetDispatchHistoryQueryHandler : IRequestHandler<GetDispatchHistoryQuery, GetDispatchHistoryResponse>
{
    private readonly IAppDbContext _context;
    private readonly ILogger<GetDispatchHistoryQueryHandler> _logger;

    public GetDispatchHistoryQueryHandler(
        IAppDbContext context,
        ILogger<GetDispatchHistoryQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetDispatchHistoryResponse> Handle(GetDispatchHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting dispatch history with filters: NotificationId={NotificationId}, UserId={UserId}, Channel={Channel}, Status={Status}",
                request.NotificationId, request.UserId, request.Channel, request.Status);

            // Build the base query
            var query = _context.NotificationDispatchAttempts
                .Include(da => da.Notification)
                    .ThenInclude(n => n.User)
                .AsQueryable();

            // Apply filters
            if (request.NotificationId.HasValue)
            {
                query = query.Where(da => da.NotificationId == request.NotificationId.Value);
            }

            if (request.UserId.HasValue)
            {
                query = query.Where(da => da.Notification.UserId == request.UserId.Value);
            }

            if (request.Channel.HasValue)
            {
                query = query.Where(da => da.Channel == request.Channel.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(da => da.Status == request.Status.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(da => da.AttemptedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(da => da.AttemptedAt <= request.ToDate.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and ordering
            var items = await query
                .OrderByDescending(da => da.AttemptedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(da => new DispatchHistoryItem
                {
                    Id = da.Id,
                    NotificationId = da.NotificationId,
                    NotificationTitle = da.Notification.Title,
                    UserId = da.Notification.UserId,
                    UserEmail = da.Notification.User.Email,
                    Channel = da.Channel,
                    Status = da.Status,
                    AttemptedAt = da.AttemptedAt.DateTime,
                    DeliveredAt = da.DeliveredAt.HasValue ? da.DeliveredAt.Value.DateTime : (DateTime?)null,
                    NextRetryAt = da.NextRetryAt.HasValue ? da.NextRetryAt.Value.DateTime : (DateTime?)null,
                    AttemptNumber = da.AttemptNumber,
                    ExternalId = da.ExternalId,
                    ErrorMessage = da.ErrorMessage,
                    Priority = (NotificationPriority)da.Notification.Priority
                })
                .ToListAsync(cancellationToken);

            // Calculate statistics
            var statistics = await CalculateStatistics(query, cancellationToken);

            var response = new GetDispatchHistoryResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                Statistics = statistics
            };

            _logger.LogInformation("Retrieved {ItemCount} dispatch history items out of {TotalCount} total",
                items.Count, totalCount);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dispatch history");
            throw;
        }
    }

    private async Task<DispatchStatistics> CalculateStatistics(
        IQueryable<Domain.Entities.NotificationDispatchAttempt> query,
        CancellationToken cancellationToken)
    {
        var allAttempts = await query.ToListAsync(cancellationToken);

        var statistics = new DispatchStatistics
        {
            TotalAttempts = allAttempts.Count,
            SuccessfulDeliveries = allAttempts.Count(da => da.Status == DispatchStatus.Delivered),
            FailedAttempts = allAttempts.Count(da => da.Status == DispatchStatus.Failed),
            PendingAttempts = allAttempts.Count(da => da.Status == DispatchStatus.Pending),
            PermanentFailures = allAttempts.Count(da => da.Status == DispatchStatus.PermanentlyFailed),
            AttemptsByChannel = allAttempts
                .GroupBy(da => da.Channel)
                .ToDictionary(g => g.Key, g => g.Count()),
            AttemptsByStatus = allAttempts
                .GroupBy(da => da.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        // Calculate success rate
        if (statistics.TotalAttempts > 0)
        {
            statistics.SuccessRate = (double)statistics.SuccessfulDeliveries / statistics.TotalAttempts * 100;
        }

        // Calculate average delivery time for successful deliveries
        var successfulDeliveries = allAttempts
            .Where(da => da.Status == DispatchStatus.Delivered && da.DeliveredAt.HasValue)
            .ToList();

        if (successfulDeliveries.Any())
        {
            statistics.AverageDeliveryTime = successfulDeliveries
                .Average(da => (da.DeliveredAt!.Value - da.AttemptedAt).TotalMinutes);
        }

        return statistics;
    }
}