using System;
using System.Threading.Tasks;
using CRM.Application.Auth.Commands;
using CRM.Application.Auth.Commands.Handlers;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CRM.Tests.Auth;

public class LogoutCommandHandlerTests
{
    private static AppDbContext MakeDb()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddEntityFrameworkInMemoryDatabase();
        var provider = services.BuildServiceProvider();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseInternalServiceProvider(provider)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Logout_Revokes_Refresh_Token()
    {
        using var db = MakeDb();
        var userId = Guid.NewGuid();
        db.RefreshTokens.Add(new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = userId,
            TokenJti = Guid.NewGuid().ToString(),
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var token = await db.RefreshTokens.FirstAsync();

        var handler = new LogoutCommandHandler(db);
        await handler.Handle(new LogoutCommand { UserId = userId }, token.TokenJti);

        var refreshed = await db.RefreshTokens.FirstAsync(r => r.RefreshTokenId == token.RefreshTokenId);
        Assert.True(refreshed.IsRevoked);
        Assert.NotNull(refreshed.RevokedAt);
    }
}
