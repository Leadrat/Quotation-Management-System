using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Clients.Queries;
using CRM.Application.Clients.Queries.Handlers;
using CRM.Application.Mapping;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using CRM.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRM.Tests.Clients
{
    public class UserActivityQueryTests
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
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new ClientHistoryProfile()));
            return new Mapper(config);
        }

        private static HistorySettings NewSettings()
        {
            return new HistorySettings
            {
                DefaultPageSize = 20,
                MaxPageSize = 100
            };
        }

        [Fact]
        public async Task User_Can_View_Own_Activity()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var settings = Options.Create(NewSettings());
            var handler = new GetUserActivityQueryHandler(db, mapper, settings);

            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();

            db.Users.Add(new User { UserId = userId, Email = "user@test.com", FirstName = "Test", LastName = "User" });
            db.Clients.Add(new Client
            {
                ClientId = clientId,
                CompanyName = "Test Corp",
                Email = "test@corp.com",
                Mobile = "+1234567890",
                CreatedByUserId = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            db.ClientHistories.Add(new ClientHistory
            {
                HistoryId = Guid.NewGuid(),
                ClientId = clientId,
                ActorUserId = userId,
                ActionType = "CREATED",
                ChangedFields = new List<string>(),
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();

            var query = new GetUserActivityQuery
            {
                UserId = userId,
                RequestorUserId = userId,
                RequestorRole = "SalesRep",
                PageNumber = 1,
                PageSize = 20
            };

            var result = await handler.Handle(query);

            Assert.True(result.TotalCount >= 1);
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public async Task User_Cannot_View_Other_User_Activity()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var settings = Options.Create(NewSettings());
            var handler = new GetUserActivityQueryHandler(db, mapper, settings);

            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            var query = new GetUserActivityQuery
            {
                UserId = userId2,
                RequestorUserId = userId1,
                RequestorRole = "SalesRep",
                PageNumber = 1,
                PageSize = 20
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await handler.Handle(query);
            });
        }

        [Fact]
        public async Task Admin_Can_View_Any_User_Activity()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var settings = Options.Create(NewSettings());
            var handler = new GetUserActivityQueryHandler(db, mapper, settings);

            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();

            db.Users.Add(new User { UserId = userId, Email = "user@test.com", FirstName = "Test", LastName = "User" });
            db.Clients.Add(new Client
            {
                ClientId = clientId,
                CompanyName = "Test Corp",
                Email = "test@corp.com",
                Mobile = "+1234567890",
                CreatedByUserId = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            db.ClientHistories.Add(new ClientHistory
            {
                HistoryId = Guid.NewGuid(),
                ClientId = clientId,
                ActorUserId = userId,
                ActionType = "CREATED",
                ChangedFields = new List<string>(),
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();

            var query = new GetUserActivityQuery
            {
                UserId = userId,
                RequestorUserId = adminId,
                RequestorRole = "Admin",
                PageNumber = 1,
                PageSize = 20
            };

            var result = await handler.Handle(query);

            Assert.True(result.TotalCount >= 1);
        }

        [Fact]
        public async Task Filter_By_ActionType_Works()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var settings = Options.Create(NewSettings());
            var handler = new GetUserActivityQueryHandler(db, mapper, settings);

            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();

            db.Users.Add(new User { UserId = userId, Email = "user@test.com", FirstName = "Test", LastName = "User" });
            db.Clients.Add(new Client
            {
                ClientId = clientId,
                CompanyName = "Test Corp",
                Email = "test@corp.com",
                Mobile = "+1234567890",
                CreatedByUserId = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            db.ClientHistories.Add(new ClientHistory
            {
                HistoryId = Guid.NewGuid(),
                ClientId = clientId,
                ActorUserId = userId,
                ActionType = "CREATED",
                ChangedFields = new List<string>(),
                CreatedAt = DateTimeOffset.UtcNow
            });
            db.ClientHistories.Add(new ClientHistory
            {
                HistoryId = Guid.NewGuid(),
                ClientId = clientId,
                ActorUserId = userId,
                ActionType = "UPDATED",
                ChangedFields = new List<string> { "Email" },
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();

            var query = new GetUserActivityQuery
            {
                UserId = userId,
                RequestorUserId = userId,
                RequestorRole = "SalesRep",
                ActionType = "CREATED",
                PageNumber = 1,
                PageSize = 20
            };

            var result = await handler.Handle(query);

            Assert.True(result.TotalCount >= 1);
            Assert.All(result.Data, entry => Assert.Equal("CREATED", entry.ActionType));
        }
    }
}

