using System.Threading.Tasks;
using CRM.Application.Users.Commands;
using CRM.Application.Users.Commands.Handlers;
using CRM.Application.Users.Commands.Results;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Filters;
using CRM.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using System;
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

    public UsersController(AppDbContext db, IAuditLogger audit, IMapper mapper, IEmailQueue emails, IResetTokenGenerator tokenGen)
    {
        _db = db;
        _audit = audit;
        _mapper = mapper;
        _emails = emails;
        _tokenGen = tokenGen;
    }

    [HttpPost]
    [AdminOnly]
    [ProducesResponseType(typeof(UserCreatedResult), 201)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand cmd)
    {
        await _audit.LogAsync("admin_create_user_attempt", new { cmd.Email, cmd.RoleId });
        var handler = new CreateUserCommandHandler(_db);
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

