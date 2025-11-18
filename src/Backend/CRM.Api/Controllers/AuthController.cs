using System;
using System.Threading.Tasks;
using CRM.Application.Users.Commands;
using CRM.Application.Users.Commands.Handlers;
using CRM.Application.Users.Commands.Results;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CRM.Infrastructure.Logging;
using CRM.Application.Auth.Commands;
using CRM.Application.Auth.Commands.Handlers;
using CRM.Application.Auth.Services;
using CRM.Shared.Config;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CRM.Application.Auth.Commands.Requests;
using CRM.Application.Common.Security;
using CRM.Application.Common.Notifications;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IPasswordHasher _hasher;
    private readonly IRefreshTokenRevoker _revoker;
    private readonly IEmailQueue _emails;

    public AuthController(AppDbContext db, IAuditLogger audit, IJwtTokenGenerator jwt, IPasswordHasher hasher, IRefreshTokenRevoker revoker, IEmailQueue emails)
    {
        _db = db;
        _audit = audit;
        _jwt = jwt;
        _hasher = hasher;
        _revoker = revoker;
        _emails = emails;
    }

    [HttpPost("register")]
    [EnableRateLimiting("register-ip")]
    [ProducesResponseType(typeof(RegisterResult), 201)]
    public async Task<IActionResult> Register([FromBody] RegisterClientCommand cmd)
    {
        await _audit.LogAsync("client_register_attempt", new { cmd.Email });
        var handler = new RegisterClientCommandHandler(_db);
        var result = await handler.Handle(cmd);
        await _audit.LogAsync("client_register_success", new { result.UserId, result.Email });
        return Created($"/api/v1/users/{result.UserId}", result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(CRM.Application.Auth.Commands.Results.LoginResult), 200)]
    public async Task<IActionResult> Login([FromBody] LoginCommand cmd)
    {
        await _audit.LogAsync("auth_login_attempt", new { cmd.Email });
        var handler = new LoginCommandHandler(_db, _jwt);
        var result = await handler.Handle(cmd);
        // set refresh cookie (preferred for browsers)
        Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
        await _audit.LogAsync("auth_login_success", new { Email = cmd.Email });
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(CRM.Application.Auth.Commands.Results.RefreshTokenResult), 200)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand cmd)
    {
        var cookieToken = Request.Cookies.TryGetValue("refreshToken", out var cookieVal) ? cookieVal : null;
        var handler = new RefreshTokenCommandHandler(_db, _jwt);
        var result = await handler.Handle(cmd, cookieToken);
        // rotate cookie
        Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand? cmd)
    {
        try
        {
            // try to get jti from cookie token
            string? jti = null;
            if (Request.Cookies.TryGetValue("refreshToken", out var cookieToken))
            {
                try
                {
                    var principal = _jwt.ValidateToken(cookieToken, validateLifetime: false);
                    jti = principal != null ? _jwt.GetJtiFromToken(principal) : null;
                }
                catch
                {
                    // Invalid token, continue with logout anyway
                }
            }
            
            if (cmd != null)
            {
                var handler = new LogoutCommandHandler(_db);
                await handler.Handle(cmd, jti);
            }
            
            // clear cookie
            Response.Cookies.Append("refreshToken", string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
            return Ok(new { success = true, message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            // Even if logout fails, clear the cookie
            Response.Cookies.Append("refreshToken", string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest body)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var cmd = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = body.CurrentPassword,
            NewPassword = body.NewPassword,
            ConfirmPassword = body.ConfirmPassword
        };
        await _audit.LogAsync("auth_change_password_attempt", new { userId });
        var handler = new ChangePasswordCommandHandler(_db, _hasher, _revoker, _emails);
        await handler.Handle(cmd);
        await _audit.LogAsync("auth_change_password_success", new { userId });
        return Ok(new { success = true });
    }
}
