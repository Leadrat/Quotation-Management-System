using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Notifications;
using CRM.Application.Users.Commands;
using CRM.Application.Users.Commands.Handlers;
using CRM.Application.Users.Validators;
using CRM.Application.Users.Dtos;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Users
{
    public class UpdateUserProfileTests
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

        [Fact]
        public async Task Handler_Updates_Profile_Successfully()
        {
            using var db = NewDb();
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = "jane@example.com",
                FirstName = "Jane",
                LastName = "Old",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var cmd = new UpdateUserProfileCommand
            {
                UserId = user.UserId,
                FirstName = "Jane",
                LastName = "Doe",
                PhoneCode = "+1",
                Mobile = "+12025550123",
                ActorUserId = user.UserId,
                IsAdminActor = false
            };

            var handler = new UpdateUserProfileCommandHandler(db, NewMapper(), new TestEmailQueue());
            var result = await handler.Handle(cmd);

            Assert.IsType<UserSummary>(result);
            Assert.Equal("Doe", (await db.Users.FirstAsync()).LastName);
        }

        [Fact]
        public void Validator_Fails_On_Bad_Mobile()
        {
            var v = new UpdateUserProfileValidator();
            var res = v.Validate(new UpdateUserProfileCommand
            {
                FirstName = "John",
                LastName = "Smith",
                Mobile = "12345"
            });
            Assert.False(res.IsValid);
        }
    }
}
