using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.Common.Results;
using CRM.Application.Notifications.Commands;
using CRM.Application.Notifications.Commands.Handlers;
using CRM.Application.Notifications.Dtos;
using CRM.Application.Notifications.Queries;
using CRM.Application.Notifications.Queries.Handlers;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly MarkNotificationsReadCommandHandler _markReadHandler;
        private readonly ArchiveNotificationsCommandHandler _archiveHandler;
        private readonly UnarchiveNotificationsCommandHandler _unarchiveHandler;
        private readonly UpdateNotificationPreferencesCommandHandler _updatePreferencesHandler;
        private readonly GetNotificationsQueryHandler _getNotificationsHandler;
        private readonly GetUnreadCountQueryHandler _getUnreadCountHandler;
        private readonly GetNotificationPreferencesQueryHandler _getPreferencesHandler;
        private readonly GetEntityNotificationsQueryHandler _getEntityNotificationsHandler;
        private readonly GetEmailNotificationLogsQueryHandler _getEmailLogsHandler;
        private readonly IValidator<MarkNotificationsReadCommand> _markReadValidator;
        private readonly IValidator<ArchiveNotificationsCommand> _archiveValidator;
        private readonly IValidator<UnarchiveNotificationsCommand> _unarchiveValidator;
        private readonly IValidator<UpdateNotificationPreferencesCommand> _updatePreferencesValidator;
        private readonly IValidator<GetNotificationsQuery> _getNotificationsValidator;
        private readonly IValidator<GetUnreadCountQuery> _getUnreadCountValidator;
        private readonly IValidator<GetNotificationPreferencesQuery> _getPreferencesValidator;
        private readonly IValidator<GetEntityNotificationsQuery> _getEntityNotificationsValidator;
        private readonly IValidator<GetEmailNotificationLogsQuery> _getEmailLogsValidator;

        public NotificationsController(
            MarkNotificationsReadCommandHandler markReadHandler,
            ArchiveNotificationsCommandHandler archiveHandler,
            UnarchiveNotificationsCommandHandler unarchiveHandler,
            UpdateNotificationPreferencesCommandHandler updatePreferencesHandler,
            GetNotificationsQueryHandler getNotificationsHandler,
            GetUnreadCountQueryHandler getUnreadCountHandler,
            GetNotificationPreferencesQueryHandler getPreferencesHandler,
            GetEntityNotificationsQueryHandler getEntityNotificationsHandler,
            GetEmailNotificationLogsQueryHandler getEmailLogsHandler,
            IValidator<MarkNotificationsReadCommand> markReadValidator,
            IValidator<ArchiveNotificationsCommand> archiveValidator,
            IValidator<UnarchiveNotificationsCommand> unarchiveValidator,
            IValidator<UpdateNotificationPreferencesCommand> updatePreferencesValidator,
            IValidator<GetNotificationsQuery> getNotificationsValidator,
            IValidator<GetUnreadCountQuery> getUnreadCountValidator,
            IValidator<GetNotificationPreferencesQuery> getPreferencesValidator,
            IValidator<GetEntityNotificationsQuery> getEntityNotificationsValidator,
            IValidator<GetEmailNotificationLogsQuery> getEmailLogsValidator)
        {
            _markReadHandler = markReadHandler;
            _archiveHandler = archiveHandler;
            _unarchiveHandler = unarchiveHandler;
            _updatePreferencesHandler = updatePreferencesHandler;
            _getNotificationsHandler = getNotificationsHandler;
            _getUnreadCountHandler = getUnreadCountHandler;
            _getPreferencesHandler = getPreferencesHandler;
            _getEntityNotificationsHandler = getEntityNotificationsHandler;
            _getEmailLogsHandler = getEmailLogsHandler;
            _markReadValidator = markReadValidator;
            _archiveValidator = archiveValidator;
            _unarchiveValidator = unarchiveValidator;
            _updatePreferencesValidator = updatePreferencesValidator;
            _getNotificationsValidator = getNotificationsValidator;
            _getUnreadCountValidator = getUnreadCountValidator;
            _getPreferencesValidator = getPreferencesValidator;
            _getEntityNotificationsValidator = getEntityNotificationsValidator;
            _getEmailLogsValidator = getEmailLogsValidator;
        }

        private bool TryGetUserContext(out Guid userId, out string role)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            role = User.FindFirstValue("role") ?? string.Empty;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userId))
            {
                userId = Guid.Empty;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get notifications for the current user
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<NotificationDto>), 200)]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] bool? unread,
            [FromQuery] bool? archived,
            [FromQuery] string? eventType,
            [FromQuery] string? entityType,
            [FromQuery] Guid? entityId,
            [FromQuery] DateTimeOffset? dateFrom,
            [FromQuery] DateTimeOffset? dateTo,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (!TryGetUserContext(out var userId, out _))
                {
                    return Unauthorized();
                }

                var query = new GetNotificationsQuery
                {
                    Unread = unread,
                    Archived = archived,
                    EventType = eventType,
                    EntityType = entityType,
                    EntityId = entityId,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    RequestorUserId = userId
                };

                var validationResult = await _getNotificationsValidator.ValidateAsync(query);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }

                var result = await _getNotificationsHandler.Handle(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(UnreadCountDto), 200)]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                if (!TryGetUserContext(out var userId, out _))
                {
                    return Unauthorized();
                }

                var query = new GetUnreadCountQuery
                {
                    RequestorUserId = userId
                };

                var validationResult = await _getUnreadCountValidator.ValidateAsync(query);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }

                var result = await _getUnreadCountHandler.Handle(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get notification preferences for the current user
        /// </summary>
        [HttpGet("preferences")]
        [ProducesResponseType(typeof(NotificationPreferencesDto), 200)]
        public async Task<IActionResult> GetPreferences()
        {
            if (!TryGetUserContext(out var userId, out _))
            {
                return Unauthorized();
            }

            var query = new GetNotificationPreferencesQuery
            {
                RequestorUserId = userId
            };

            var validationResult = await _getPreferencesValidator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _getPreferencesHandler.Handle(query);
            return Ok(result);
        }

        /// <summary>
        /// Get notifications for a specific entity
        /// </summary>
        [HttpGet("entity/{entityType}/{entityId}")]
        [ProducesResponseType(typeof(NotificationDto[]), 200)]
        public async Task<IActionResult> GetEntityNotifications(string entityType, Guid entityId)
        {
            if (!TryGetUserContext(out var userId, out _))
            {
                return Unauthorized();
            }

            var query = new GetEntityNotificationsQuery
            {
                EntityType = entityType,
                EntityId = entityId,
                RequestorUserId = userId
            };

            var validationResult = await _getEntityNotificationsValidator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _getEntityNotificationsHandler.Handle(query);
            return Ok(result);
        }

        /// <summary>
        /// Mark notifications as read
        /// </summary>
        [HttpPost("mark-read")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> MarkRead([FromBody] MarkNotificationsReadRequest request)
        {
            if (!TryGetUserContext(out var userId, out _))
            {
                return Unauthorized();
            }

            var command = new MarkNotificationsReadCommand
            {
                NotificationIds = request.NotificationIds,
                RequestedByUserId = userId
            };

            var validationResult = await _markReadValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var count = await _markReadHandler.Handle(command);
            return Ok(new { success = true, count });
        }

        /// <summary>
        /// Archive notifications
        /// </summary>
        [HttpPost("archive")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> Archive([FromBody] ArchiveNotificationsRequest request)
        {
            if (!TryGetUserContext(out var userId, out _))
            {
                return Unauthorized();
            }

            var command = new ArchiveNotificationsCommand
            {
                NotificationIds = request.NotificationIds,
                RequestedByUserId = userId
            };

            var validationResult = await _archiveValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var count = await _archiveHandler.Handle(command);
            return Ok(new { success = true, count });
        }

        /// <summary>
        /// Unarchive notifications
        /// </summary>
        [HttpPost("unarchive")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> Unarchive([FromBody] UnarchiveNotificationsRequest request)
        {
            if (!TryGetUserContext(out var userId, out _))
            {
                return Unauthorized();
            }

            var command = new UnarchiveNotificationsCommand
            {
                NotificationIds = request.NotificationIds,
                RequestedByUserId = userId
            };

            var validationResult = await _unarchiveValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var count = await _unarchiveHandler.Handle(command);
            return Ok(new { success = true, count });
        }

        /// <summary>
        /// Update notification preferences
        /// </summary>
        [HttpPut("preferences")]
        [ProducesResponseType(typeof(NotificationPreferencesDto), 200)]
        public async Task<IActionResult> UpdatePreferences([FromBody] UpdateNotificationPreferencesRequest request)
        {
            if (!TryGetUserContext(out var userId, out _))
            {
                return Unauthorized();
            }

            var command = new UpdateNotificationPreferencesCommand
            {
                UserId = userId,
                Preferences = request.Preferences
            };

            var validationResult = await _updatePreferencesValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _updatePreferencesHandler.Handle(command);
            return Ok(result);
        }

        /// <summary>
        /// Get email notification logs (Admin only)
        /// </summary>
        [HttpGet("email-logs")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PagedResult<EmailNotificationLogDto>), 200)]
        public async Task<IActionResult> GetEmailLogs(
            [FromQuery] Guid? userId,
            [FromQuery] string? recipientEmail,
            [FromQuery] string? eventType,
            [FromQuery] string? status,
            [FromQuery] DateTimeOffset? dateFrom,
            [FromQuery] DateTimeOffset? dateTo,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (!TryGetUserContext(out var requestorUserId, out var role))
            {
                return Unauthorized();
            }

            var query = new GetEmailNotificationLogsQuery
            {
                UserId = userId,
                RecipientEmail = recipientEmail,
                EventType = eventType,
                Status = status,
                DateFrom = dateFrom,
                DateTo = dateTo,
                PageNumber = pageNumber,
                PageSize = pageSize,
                RequestorUserId = requestorUserId,
                RequestorRole = role
            };

            var validationResult = await _getEmailLogsValidator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _getEmailLogsHandler.Handle(query);
            return Ok(result);
        }
    }
}

