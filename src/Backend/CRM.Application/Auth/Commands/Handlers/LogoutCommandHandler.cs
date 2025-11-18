using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Auth.Commands.Handlers;

public class LogoutCommandHandler
{
    private readonly IAppDbContext _db;

    public LogoutCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(LogoutCommand cmd, string? cookieRefreshTokenJti)
    {
        // Prefer cookie JTI if provided; otherwise try to parse from token (not implemented here)
        var jti = cookieRefreshTokenJti;
        if (string.IsNullOrWhiteSpace(jti))
        {
            // best-effort: nothing to revoke if missing
            return;
        }
        var now = DateTime.UtcNow;
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenJti == jti && r.UserId == cmd.UserId);
        if (token == null)
        {
            return; // idempotent
        }
        token.IsRevoked = true;
        token.RevokedAt = now;
        await _db.SaveChangesAsync();
    }
}
