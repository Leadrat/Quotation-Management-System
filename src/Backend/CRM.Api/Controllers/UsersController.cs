using System.Threading.Tasks;
using CRM.Application.Users.Commands;
using CRM.Application.Users.Commands.Handlers;
using CRM.Application.Users.Commands.Results;
using CRM.Application.Users.Queries;
using CRM.Application.Users.Queries.Handlers;
using CRM.Application.Common.Interfaces;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Filters;
using CRM.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using AutoMapper;
using CRM.Application.Users.Commands.Requests;
using CRM.Application.Users.Dtos;
using System.Security.Claims;
using CRM.Application.Common.Notifications;
using CRM.Application.Common.Security;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly IMapper _mapper;
    private readonly IEmailQueue _emails;
    private readonly IResetTokenGenerator _tokenGen;
    private readonly ITenantContext _tenantContext;

    public UsersController(AppDbContext db, IAuditLogger audit, IMapper mapper, IEmailQueue emails, IResetTokenGenerator tokenGen, ITenantContext tenantContext)
    {
        _db = db;
        _audit = audit;
        _mapper = mapper;
        _emails = emails;
        _tokenGen = tokenGen;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
    {
        var user = HttpContext.User;
        var isAdmin = user.IsInRole("Admin");
        var role = isAdmin ? "Admin" : (user.IsInRole("SalesRep") ? "SalesRep" : string.Empty);
        Guid.TryParse(user.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userId" || c.Type == ClaimTypes.NameIdentifier)?.Value, out var currentUserId);

        var query = new GetAllUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            RequestorUserId = currentUserId,
            RequestorRole = role
        };

        var handler = new GetAllUsersQueryHandler(_db, _mapper, _tenantContext);
        var result = await handler.Handle(query);

        return Ok(new
        {
            success = true,
            data = result.Data,
            pageNumber = result.PageNumber,
            pageSize = result.PageSize,
            totalCount = result.TotalCount
        });
    }

    [HttpPost]
    [AdminOnly]
    [ProducesResponseType(typeof(UserCreatedResult), 201)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand cmd)
    {
        var actorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        Guid.TryParse(actorIdStr, out var actorId);
        cmd.CreatedByUserId = actorId;

        await _audit.LogAsync("admin_create_user_attempt", new { cmd.Email, cmd.RoleId });
        var handler = new CreateUserCommandHandler(_db, _tenantContext);
        var result = await handler.Handle(cmd);
        await _audit.LogAsync("admin_create_user_success", new { result.UserId, result.Email, result.RoleId });
        return Created(string.Empty, result);
    }

    [HttpPut("{userId}/profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserSummary), 200)]
    public async Task<IActionResult> UpdateProfile([FromRoute] Guid userId, [FromBody] UpdateUserProfileRequest body)
    {
        var actorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        var isAdmin = string.Equals(User.FindFirstValue("role"), "Admin", StringComparison.OrdinalIgnoreCase);
        _ = Guid.TryParse(actorIdStr, out var actorId);

        var cmd = new UpdateUserProfileCommand
        {
            UserId = userId,
            FirstName = body.FirstName,
            LastName = body.LastName,
            PhoneCode = body.PhoneCode,
            Mobile = body.Mobile,
            ActorUserId = actorId,
            IsAdminActor = isAdmin
        };

        await _audit.LogAsync("user_update_profile_attempt", new { actorId, userId });
        var handler = new UpdateUserProfileCommandHandler(_db, _mapper, _emails);
        var result = await handler.Handle(cmd);
        await _audit.LogAsync("user_update_profile_success", new { actorId, userId });
        return Ok(result);
    }

    [HttpPost("{userId}/reset-password")]
    [AdminOnly]
    public async Task<IActionResult> ResetPassword([FromRoute] Guid userId)
    {
        await _audit.LogAsync("admin_reset_password_attempt", new { userId });
        var actorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        _ = Guid.TryParse(actorIdStr, out var adminId);
        var handler = new CRM.Application.Auth.Commands.Handlers.InitiatePasswordResetCommandHandler(_db, _tokenGen, _emails);
        await handler.Handle(new CRM.Application.Auth.Commands.InitiatePasswordResetCommand
        {
            TargetUserId = userId,
            AdminUserId = adminId
        });
        await _audit.LogAsync("admin_reset_password_enqueued", new { userId });
        return Accepted(new { success = true });
    }
}

