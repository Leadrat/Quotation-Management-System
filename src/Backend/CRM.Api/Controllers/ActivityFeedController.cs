using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Queries;
using CRM.Application.UserManagement.Queries.Handlers;
using CRM.Application.Common.Results;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/v1/activity-feed")]
[Authorize]
public class ActivityFeedController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public ActivityFeedController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetActivityFeed(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] Guid? userId = null, [FromQuery] string? actionType = null,
        [FromQuery] string? entityType = null, [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        var handler = new GetActivityFeedQueryHandler(_db, _mapper);
        var result = await handler.Handle(new GetActivityFeedQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            UserId = userId,
            ActionType = actionType,
            EntityType = entityType,
            FromDate = fromDate,
            ToDate = toDate,
            RequestorUserId = requestorId,
            RequestorRole = role
        });
        return Ok(result);
    }

    [HttpGet("users/{userId}/activity")]
    public async Task<IActionResult> GetUserActivity([FromRoute] Guid userId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? actionType = null, [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var requestorId)) return Unauthorized();
        var role = User.FindFirstValue("role") ?? string.Empty;

        var handler = new GetUserActivityQueryHandler(_db, _mapper);
        try
        {
            var result = await handler.Handle(new GetUserActivityQuery
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                ActionType = actionType,
                FromDate = fromDate,
                ToDate = toDate,
                RequestorUserId = requestorId,
                RequestorRole = role
            });
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}

