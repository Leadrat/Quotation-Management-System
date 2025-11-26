using CRM.Application.Notifications.Queries;
using CRM.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for notification dispatch management and reporting
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationDispatchController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationDispatchController> _logger;

    public NotificationDispatchController(
        IMediator mediator,
        ILogger<NotificationDispatchController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets dispatch history with optional filtering
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<GetDispatchHistoryResponse>> GetDispatchHistory(
        [FromQuery] Guid? notificationId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] NotificationChannel? channel = null,
        [FromQuery] DispatchStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetDispatchHistoryQuery
            {
                NotificationId = notificationId,
                UserId = userId,
                Channel = channel,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = Math.Min(pageSize, 100) // Cap at 100 items per page
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dispatch history");
            return StatusCode(500, "An error occurred while retrieving dispatch history");
        }
    }

    /// <summary>
    /// Gets dispatch statistics for a specific time period
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<DispatchStatistics>> GetDispatchStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] NotificationChannel? channel = null)
    {
        try
        {
            // Default to last 24 hours if no dates provided
            var endDate = toDate ?? DateTime.UtcNow;
            var startDate = fromDate ?? endDate.AddDays(-1);

            var query = new GetDispatchHistoryQuery
            {
                Channel = channel,
                FromDate = startDate,
                ToDate = endDate,
                Page = 1,
                PageSize = 1 // We only need statistics, not the items
            };

            var result = await _mediator.Send(query);
            return Ok(result.Statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dispatch statistics");
            return StatusCode(500, "An error occurred while retrieving dispatch statistics");
        }
    }

    /// <summary>
    /// Gets dispatch history for a specific notification
    /// </summary>
    [HttpGet("notification/{notificationId}")]
    public async Task<ActionResult<GetDispatchHistoryResponse>> GetNotificationDispatchHistory(
        Guid notificationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetDispatchHistoryQuery
            {
                NotificationId = notificationId,
                Page = page,
                PageSize = Math.Min(pageSize, 100)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dispatch history for notification {NotificationId}", notificationId);
            return StatusCode(500, "An error occurred while retrieving notification dispatch history");
        }
    }

    /// <summary>
    /// Gets dispatch history for a specific user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<GetDispatchHistoryResponse>> GetUserDispatchHistory(
        Guid userId,
        [FromQuery] NotificationChannel? channel = null,
        [FromQuery] DispatchStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetDispatchHistoryQuery
            {
                UserId = userId,
                Channel = channel,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = Math.Min(pageSize, 100)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dispatch history for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user dispatch history");
        }
    }

    /// <summary>
    /// Gets failed dispatches that may need attention
    /// </summary>
    [HttpGet("failed")]
    public async Task<ActionResult<GetDispatchHistoryResponse>> GetFailedDispatches(
        [FromQuery] NotificationChannel? channel = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetDispatchHistoryQuery
            {
                Channel = channel,
                Status = DispatchStatus.Failed,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = Math.Min(pageSize, 100)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving failed dispatches");
            return StatusCode(500, "An error occurred while retrieving failed dispatches");
        }
    }

    /// <summary>
    /// Gets permanently failed dispatches
    /// </summary>
    [HttpGet("permanently-failed")]
    public async Task<ActionResult<GetDispatchHistoryResponse>> GetPermanentlyFailedDispatches(
        [FromQuery] NotificationChannel? channel = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetDispatchHistoryQuery
            {
                Channel = channel,
                Status = DispatchStatus.PermanentlyFailed,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = Math.Min(pageSize, 100)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permanently failed dispatches");
            return StatusCode(500, "An error occurred while retrieving permanently failed dispatches");
        }
    }
}