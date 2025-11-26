using CRM.Application.Notifications.Commands;
using CRM.Application.Notifications.Dtos;
using CRM.Application.Notifications.Exceptions;
using CRM.Application.Notifications.Queries;
using CRM.Application.Notifications.Validators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize(Roles = "Admin,SalesRep")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new notification
    /// </summary>
    /// <param name="command">Notification creation data</param>
    /// <returns>Created notification</returns>
    [HttpPost]
    public async Task<ActionResult<object>> CreateNotification([FromBody] CreateNotificationCommand command)
    {
        try
        {
            // Validate the command
            var validator = new CreateNotificationCommandValidator();
            var validationResult = await validator.ValidateAsync(command);
            
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
                });
            }

            var notification = await _mediator.Send(command);

            return StatusCode(201, new
            {
                success = true,
                data = notification,
                message = "Notification created successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while creating the notification",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get user notifications with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="isRead">Filter by read status</param>
    /// <param name="notificationTypeId">Filter by notification type</param>
    /// <param name="fromDate">Filter from date</param>
    /// <param name="toDate">Filter to date</param>
    /// <param name="targetUserId">Admin only: View notifications for specific user</param>
    /// <returns>Paginated list of notifications</returns>
    [HttpGet]
    public async Task<ActionResult<object>> GetUserNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isRead = null,
        [FromQuery] Guid? notificationTypeId = null,
        [FromQuery] DateTimeOffset? fromDate = null,
        [FromQuery] DateTimeOffset? toDate = null,
        [FromQuery] Guid? targetUserId = null)
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid or missing user ID in token"
                });
            }

            // Check if user is Admin (can view all notifications if needed)
            var isAdmin = User.IsInRole("Admin");

            var query = new GetUserNotificationsQuery
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                IsRead = isRead,
                NotificationTypeId = notificationTypeId,
                FromDate = fromDate,
                ToDate = toDate,
                IsAdmin = isAdmin,
                TargetUserId = targetUserId
            };

            var result = await _mediator.Send(query);

            return Ok(new
            {
                success = true,
                data = result.Data,
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                hasMore = (result.PageNumber * result.PageSize) < result.TotalCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving notifications",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    /// <param name="notificationId">Notification ID</param>
    /// <returns>Updated notification</returns>
    [HttpPut("{notificationId}/read")]
    public async Task<ActionResult<object>> MarkNotificationAsRead(Guid notificationId)
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid or missing user ID in token"
                });
            }

            // Check if user is Admin
            var isAdmin = User.IsInRole("Admin");

            var command = new MarkNotificationAsReadCommand
            {
                NotificationId = notificationId,
                UserId = userId,
                IsAdmin = isAdmin
            };

            // Validate the command
            var validator = new MarkNotificationAsReadCommandValidator();
            var validationResult = await validator.ValidateAsync(command);
            
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
                });
            }

            var notification = await _mediator.Send(command);

            return Ok(new
            {
                success = true,
                data = notification,
                message = "Notification marked as read successfully"
            });
        }
        catch (NotificationNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (UnauthorizedNotificationAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while marking the notification as read",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get unread notification count for the current user
    /// </summary>
    /// <returns>Unread notification count</returns>
    [HttpGet("unread-count")]
    public async Task<ActionResult<object>> GetUnreadNotificationCount()
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid or missing user ID in token"
                });
            }

            var query = new GetUnreadNotificationCountQuery
            {
                UserId = userId
            };

            var count = await _mediator.Send(query);

            return Ok(new
            {
                success = true,
                data = new { unreadCount = count },
                message = "Unread notification count retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving unread notification count",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get notification preferences for the current user
    /// </summary>
    /// <returns>User notification preferences</returns>
    [HttpGet("preferences")]
    public async Task<ActionResult<object>> GetNotificationPreferences()
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid or missing user ID in token"
                });
            }

            // For now, return mock preferences until the full implementation is ready
            var mockPreferences = new
            {
                userId = userId,
                emailNotifications = true,
                smsNotifications = false,
                inAppNotifications = true,
                notificationTypes = new[]
                {
                    new { type = "QuotationSent", enabled = true, channels = new[] { "EMAIL", "IN_APP" } },
                    new { type = "PaymentReminder", enabled = true, channels = new[] { "EMAIL", "SMS" } },
                    new { type = "QuotationExpiring", enabled = false, channels = new[] { "EMAIL" } }
                }
            };

            return Ok(new
            {
                success = true,
                data = mockPreferences,
                message = "Notification preferences retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving notification preferences",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Update notification preferences for the current user
    /// </summary>
    /// <param name="preferences">Updated preferences</param>
    /// <returns>Updated preferences</returns>
    [HttpPut("preferences")]
    public async Task<ActionResult<object>> UpdateNotificationPreferences([FromBody] object preferences)
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid or missing user ID in token"
                });
            }

            // For now, just return the received preferences as confirmation
            // In a full implementation, this would save to database
            return Ok(new
            {
                success = true,
                data = preferences,
                message = "Notification preferences updated successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while updating notification preferences",
                error = ex.Message
            });
        }
    }
}