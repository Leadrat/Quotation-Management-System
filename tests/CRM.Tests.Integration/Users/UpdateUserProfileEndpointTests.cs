using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Api.Controllers;
using CRM.Application.Common.Notifications;
using CRM.Application.Users.Commands.Requests;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Integration.Users
{
    public class UpdateUserProfileEndpointTests
    {
        private static AppDbContext NewDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static IMapper NewMapper()
        {
            var cfg = new MapperConfiguration(c => c.AddProfile(new CRM.Application.Mapping.UserProfile()));
            return cfg.CreateMapper();
        }

        private class TestEmailQueue : IEmailQueue
        {
            public Task EnqueueAsync(EmailMessage message) => Task.CompletedTask;
        }

        private class AuditLogger : IAuditLogger
        {
            public Task LogAsync(string eventName, object data) => Task.CompletedTask;
        }

        [Fact]
        public async Task UpdateProfile_Returns_Ok()
        {
            using var db = NewDb();
            var user = new CRM.Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                Email = "jane@example.com",
                FirstName = "Jane",
                LastName = "Old"
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var controller = new UsersController(db, new AuditLogger(), NewMapper(), new TestEmailQueue(), new CRM.Infrastructure.Security.ResetTokenGenerator());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim("role", "SalesRep")
                    }, "TestAuth"))
                }
            };

            var body = new UpdateUserProfileRequest { FirstName = "Jane", LastName = "Doe", PhoneCode = "+1", Mobile = "+12025550123" };
            var result = await controller.UpdateProfile(user.UserId, body) as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }
    }
}
