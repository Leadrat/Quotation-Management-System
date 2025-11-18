using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Clients.Commands;
using CRM.Application.Clients.Commands.Handlers;
using CRM.Application.Clients.Dtos;
using CRM.Application.Clients.Exceptions;
using CRM.Application.Clients.Queries;
using CRM.Application.Clients.Queries.Handlers;
using CRM.Application.Clients.Services;
using CRM.Application.Clients.Validation;
using CRM.Application.Clients.Validators;
using CRM.Application.Common.Results;
using CRM.Application.Common.Validation;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using CRM.Shared.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/clients")]
    public class ClientsHistoryController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IOptions<HistorySettings> _historySettings;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<ClientsHistoryController> _logger;

        public ClientsHistoryController(
            AppDbContext db,
            IMapper mapper,
            IMemoryCache cache,
            IOptions<HistorySettings> historySettings,
            IAuditLogger auditLogger,
            ILogger<ClientsHistoryController> logger)
        {
            _db = db;
            _mapper = mapper;
            _cache = cache;
            _historySettings = historySettings;
            _auditLogger = auditLogger;
            _logger = logger;
        }

        [HttpGet("{clientId:guid}/history")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> GetHistory(
            [FromRoute] Guid clientId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool includeAccessLogs = false)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var role = GetRole();

            var query = new GetClientHistoryQuery
            {
                ClientId = clientId,
                RequestorUserId = userId.Value,
                RequestorRole = role,
                PageNumber = pageNumber,
                PageSize = pageSize,
                IncludeAccessLogs = includeAccessLogs
            };

            var validator = new GetClientHistoryQueryValidator();
            var validation = validator.Validate(query);
            if (!validation.IsValid)
            {
                return BadRequest(new { success = false, error = "Validation failed", errors = validation.ToDictionary() });
            }

            var handler = new GetClientHistoryQueryHandler(_db, _mapper, _historySettings);
            PagedResult<Application.Clients.Dtos.ClientHistoryEntryDto> result;

            try
            {
                _logger.LogInformation("Fetching history for client {ClientId} by user {UserId}", clientId, userId);
                result = await handler.Handle(query);
                await _auditLogger.LogAsync("client_history_viewed", new { clientId, userId, pageNumber, pageSize });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Unauthorized history access attempt for client {ClientId} by user {UserId}", clientId, userId);
                return Forbid();
            }

            var hasMore = (result.PageNumber * result.PageSize) < result.TotalCount;
            return Ok(new
            {
                success = true,
                data = result.Data,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalCount = result.TotalCount,
                hasMore
            });
        }

        [HttpGet("{clientId:guid}/timeline")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> GetTimeline([FromRoute] Guid clientId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var role = GetRole();

            var query = new GetClientTimelineQuery
            {
                ClientId = clientId,
                RequestorUserId = userId.Value,
                RequestorRole = role
            };

            var validator = new GetClientTimelineQueryValidator();
            var validation = validator.Validate(query);
            if (!validation.IsValid)
            {
                return BadRequest(new { success = false, error = "Validation failed", errors = validation.ToDictionary() });
            }

            var cacheKey = $"client-timeline:{clientId}:{userId}:{role}";
            if (!_cache.TryGetValue(cacheKey, out Application.Clients.Dtos.ClientTimelineSummaryDto? cached))
            {
                var handler = new GetClientTimelineQueryHandler(_db, _mapper, _historySettings);
                try
                {
                    cached = await handler.Handle(query);
                }
                catch (UnauthorizedAccessException)
                {
                    return Forbid();
                }

                var cacheEntry = _cache.CreateEntry(cacheKey);
                cacheEntry.Value = cached;
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
                cacheEntry.Dispose();
            }

            return Ok(new { success = true, data = cached });
        }

        [HttpPost("{clientId:guid}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore([FromRoute] Guid clientId, [FromBody] RestoreClientBody body)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var command = new RestoreClientCommand
            {
                ClientId = clientId,
                RequestorUserId = userId.Value,
                RequestorRole = "Admin",
                Reason = body.Reason ?? string.Empty,
                MetadataJson = BuildMetadataJson()
            };

            var validator = new RestoreClientCommandValidator();
            var validation = validator.Validate(command);
            if (!validation.IsValid)
            {
                return BadRequest(new { success = false, error = "Validation failed", errors = validation.ToDictionary() });
            }

            var handler = new RestoreClientCommandHandler(_db, _mapper, _historySettings, new ClientHistoryDiffBuilder());

            try
            {
                _logger.LogInformation("Restoring client {ClientId} by admin {UserId}", clientId, userId);
                var dto = await handler.Handle(command);
                await _auditLogger.LogAsync("client_restored", new { clientId, userId, reason = body.Reason });
                return Ok(new { success = true, message = "Client restored successfully", data = dto });
            }
            catch (ClientNotFoundException)
            {
                return NotFound(new { success = false, error = "Client not found" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        private Guid? GetUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var userId) ? userId : null;
        }

        private string GetRole()
        {
            return User.FindFirstValue("role") ?? string.Empty;
        }

        private string BuildMetadataJson()
        {
            var httpContext = HttpContext;
            var metadata = new HistoryMetadataDto
            {
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                UserAgent = httpContext.Request.Headers["User-Agent"].ToString(),
                IsAutomation = false,
                RequestId = httpContext.TraceIdentifier,
                Origin = httpContext.Request.Headers["Origin"].ToString()
            };
            return JsonSerializer.Serialize(metadata);
        }

        [HttpGet("users/{userId:guid}/activity")]
        [Authorize(Roles = "SalesRep,Admin,Manager")]
        public async Task<IActionResult> GetUserActivity(
            [FromRoute] Guid userId,
            [FromQuery] string? actionType = null,
            [FromQuery] DateTimeOffset? dateFrom = null,
            [FromQuery] DateTimeOffset? dateTo = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var requestorUserId = GetUserId();
            if (requestorUserId == null) return Unauthorized();
            var role = GetRole();

            var query = new GetUserActivityQuery
            {
                UserId = userId,
                RequestorUserId = requestorUserId.Value,
                RequestorRole = role,
                ActionType = actionType,
                DateFrom = dateFrom,
                DateTo = dateTo,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var validator = new GetUserActivityQueryValidator();
            var validation = validator.Validate(query);
            if (!validation.IsValid)
            {
                return BadRequest(new { success = false, error = "Validation failed", errors = validation.ToDictionary() });
            }

            var handler = new GetUserActivityQueryHandler(_db, _mapper, _historySettings);
            PagedResult<ClientHistoryEntryDto> result;

            try
            {
                result = await handler.Handle(query);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            var hasMore = (result.PageNumber * result.PageSize) < result.TotalCount;
            return Ok(new
            {
                success = true,
                data = result.Data,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalCount = result.TotalCount,
                hasMore
            });
        }

        [HttpGet("history/export")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ExportHistory(
            [FromQuery] string? clientIds = null,
            [FromQuery] string? actionType = null,
            [FromQuery] DateTimeOffset? dateFrom = null,
            [FromQuery] DateTimeOffset? dateTo = null,
            [FromQuery] string format = "csv")
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var role = GetRole();

            var clientIdList = new List<Guid>();
            if (!string.IsNullOrWhiteSpace(clientIds))
            {
                foreach (var idStr in clientIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (Guid.TryParse(idStr.Trim(), out var clientId))
                    {
                        clientIdList.Add(clientId);
                    }
                }
            }

            var query = new ExportClientHistoryQuery
            {
                ClientIds = clientIdList.Any() ? clientIdList : null,
                ActionType = actionType,
                DateFrom = dateFrom,
                DateTo = dateTo,
                RequestorUserId = userId.Value,
                RequestorRole = role,
                MaxRows = 5000
            };

            var handler = new ExportClientHistoryQueryHandler(_db, _mapper);
            IEnumerable<ClientHistoryEntryDto> data;

            try
            {
                data = await handler.Handle(query);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { success = false, error = "PDF export not available in Phase 1. Use format=csv." });
            }

            var csvWriter = new ClientHistoryCsvWriter();
            var stream = new MemoryStream();
            await csvWriter.WriteToStreamAsync(stream, data, 5000);
            stream.Position = 0;

            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
            return File(stream, "text/csv", $"client_history_{timestamp}.csv");
        }

        [HttpGet("admin/suspicious-activity")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSuspiciousActivity(
            [FromQuery] short minScore = 7,
            [FromQuery] string? status = null,
            [FromQuery] DateTimeOffset? dateFrom = null,
            [FromQuery] DateTimeOffset? dateTo = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetSuspiciousActivityQuery
            {
                MinScore = minScore,
                Status = status,
                DateFrom = dateFrom,
                DateTo = dateTo,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var handler = new GetSuspiciousActivityQueryHandler(_db, _mapper, _historySettings);
            var result = await handler.Handle(query);

            var hasMore = (result.PageNumber * result.PageSize) < result.TotalCount;
            return Ok(new
            {
                success = true,
                data = result.Data,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalCount = result.TotalCount,
                hasMore
            });
        }

        public class RestoreClientBody
        {
            public string Reason { get; set; } = string.Empty;
        }
    }
}

