using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Api.Controllers;
using CRM.Application.Auth.Commands;
using CRM.Application.Auth.Services;
using CRM.Application.Common.Notifications;
using CRM.Application.Common.Security;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using CRM.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Integration.Auth
{
    public class InitiatePasswordResetEndpointTests
    {
        private static AppDbContext NewDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private class AuditLogger : IAuditLogger
        {
            public Task LogAsync(string eventName, object data) => Task.CompletedTask;
        }

        private class TestEmailQueue : IEmailQueue
        {
            public Task EnqueueAsync(EmailMessage message) => Task.CompletedTask;
        }

        [Fact]
        public async Task Admin_ResetPassword_Returns_Accepted()
        {
            using var db = NewDb();
            var user = new CRM.Domain.Entities.User { UserId = Guid.NewGuid(), Email = "user@example.com", FirstName = "F", LastName = "L" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var controller = new CRM.Api.Controllers.UsersController(db, new AuditLogger(), new AutoMapper.Mapper(new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile(new CRM.Application.Mapping.UserProfile()))), new TestEmailQueue(), new ResetTokenGenerator());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                        new Claim("role", "Admin")
                    }, "TestAuth"))
                }
            };

            var result = await controller.ResetPassword(user.UserId) as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(202, result!.StatusCode);
        }
    }
}
