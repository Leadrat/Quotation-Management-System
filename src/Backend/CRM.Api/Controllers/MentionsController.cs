using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.Commands.Handlers;
using CRM.Application.UserManagement.Queries;
using CRM.Application.UserManagement.Queries.Handlers;
using CRM.Application.UserManagement.Requests;
using CRM.Application.UserManagement.Validators;
using CRM.Application.Common.Results;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/v1/mentions")]
[Authorize]
public class MentionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly IMapper _mapper;

    public MentionsController(AppDbContext db, IAuditLogger audit, IMapper mapper)
    {
        _db = db;
        _audit = audit;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMentionRequest body)
    {
        var validator = new CreateMentionRequestValidator();
        var result = validator.Validate(body);
        if (!result.IsValid)
        {
            return BadRequest(new { success = false, error = "Validation failed", errors = result.ToDictionary() });
        }

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

        await _audit.LogAsync("mention_create_attempt", new { userId, body.EntityType, body.EntityId, body.MentionedUserId });

        var cmd = new CreateMentionCommand
        {
            Request = body,
            MentionedByUserId = userId
        };

        var handler = new CreateMentionCommandHandler(_db, _mapper);
        try
        {
            var mention = await handler.Handle(cmd);
            await _audit.LogAsync("mention_create_success", new { userId, mention.MentionId });
            return StatusCode(201, new { success = true, message = "Mention created successfully", data = mention });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetMentions([FromRoute] Guid userId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] bool? isRead = null)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();

        var handler = new GetMentionsQueryHandler(_db, _mapper);
        try
        {
            var result = await handler.Handle(new GetMentionsQuery
            {
                UserId = userId,
                IsRead = isRead,
                PageNumber = pageNumber,
                PageSize = pageSize,
                RequestorUserId = requestorId
            });
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("user/{userId}/unread-count")]
    public async Task<IActionResult> GetUnreadCount([FromRoute] Guid userId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();

        var handler = new GetUnreadMentionsCountQueryHandler(_db);
        try
        {
            var count = await handler.Handle(new GetUnreadMentionsCountQuery
            {
                UserId = userId,
                RequestorUserId = requestorId
            });
            return Ok(new { success = true, count });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{mentionId}/mark-read")]
    public async Task<IActionResult> MarkAsRead([FromRoute] Guid mentionId)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

        await _audit.LogAsync("mention_mark_read_attempt", new { userId, mentionId });

        var cmd = new MarkMentionReadCommand
        {
            MentionId = mentionId,
            UserId = userId
        };

        var handler = new MarkMentionReadCommandHandler(_db);
        try
        {
            await handler.Handle(cmd);
            await _audit.LogAsync("mention_mark_read_success", new { userId, mentionId });
            return Ok(new { success = true, message = "Mention marked as read" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
        catch (CRM.Application.UserManagement.Exceptions.UnauthorizedTeamOperationException ex)
        {
            return Forbid(ex.Message);
        }
    }
}

