using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Api.Controllers;
using CRM.Application.Auth.Commands.Requests;
using CRM.Application.Auth.Services;
using CRM.Application.Common.Notifications;
using CRM.Application.Common.Security;
using CRM.Infrastructure.Persistence;
using CRM.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Integration.Auth
{
    public class ChangePasswordEndpointTests
    {
        private static AppDbContext NewDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private class AuditLogger : CRM.Infrastructure.Logging.IAuditLogger
        {
            public Task LogAsync(string eventName, object data) => Task.CompletedTask;
        }

        private class FakeRevoker : IRefreshTokenRevoker
        {
            public Task RevokeAllForUserAsync(Guid userId) => Task.CompletedTask;
        }

        private class FakeEmailQueue : IEmailQueue
        {
            public Task EnqueueAsync(EmailMessage message) => Task.CompletedTask;
        }

        private class FakeJwt : IJwtTokenGenerator
        {
            public string GenerateAccessToken(CRM.Domain.Entities.User user, DateTime nowUtc, string? refreshJti = null) => string.Empty;
            public (string token, string jti, DateTime expiresAtUtc) GenerateRefreshToken(Guid userId, DateTime nowUtc) => (string.Empty, string.Empty, DateTime.UtcNow);
            public System.Security.Claims.ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true) => null;
            public Guid GetUserIdFromToken(ClaimsPrincipal principal) => Guid.Empty;
            public string? GetJtiFromToken(ClaimsPrincipal principal) => null;
        }

        [Fact]
        public async Task ChangePassword_Returns_Ok()
        {
            using var db = NewDb();
            IPasswordHasher hasher = new BCryptPasswordHasher();
            var userId = Guid.NewGuid();
            db.Users.Add(new CRM.Domain.Entities.User
            {
                UserId = userId,
                Email = "john@example.com",
                FirstName = "John",
                LastName = "Smith",
                PasswordHash = hasher.Hash("OldPass!23")
            });
            await db.SaveChangesAsync();

            var controller = new AuthController(db, new AuditLogger(), new FakeJwt(), hasher, new FakeRevoker(), new FakeEmailQueue());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim("role", "SalesRep")
                    }, "TestAuth"))
                }
            };

            var body = new ChangePasswordRequest { CurrentPassword = "OldPass!23", NewPassword = "NewPass!23", ConfirmPassword = "NewPass!23" };
            var result = await controller.ChangePassword(body) as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }
    }
}
