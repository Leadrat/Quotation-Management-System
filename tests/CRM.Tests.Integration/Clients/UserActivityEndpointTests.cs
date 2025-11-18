using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Api.Controllers;
using CRM.Application.Mapping;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using CRM.Shared.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRM.Tests.Integration.Clients
{
    public class UserActivityEndpointTests
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
            var cfg = new MapperConfiguration(c =>
            {
                c.AddProfile(new ClientHistoryProfile());
            });
            return cfg.CreateMapper();
        }

        private static ClaimsPrincipal SalesRep(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim(ClaimTypes.Role, "SalesRep")
            }, "TestAuth"));
        }

        private static ClaimsPrincipal Admin(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            }, "TestAuth"));
        }

        private static ClaimsPrincipal Manager(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim(ClaimTypes.Role, "Manager")
            }, "TestAuth"));
        }

        private static ClientsHistoryController NewController(AppDbContext db, IMapper mapper, ClaimsPrincipal user)
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var settings = Options.Create(new HistorySettings { DefaultPageSize = 20, MaxPageSize = 100 });
            var ctrl = new ClientsHistoryController(db, mapper, cache, settings);
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return ctrl;
        }

        private static void SeedUserActivity(AppDbContext db, Guid userId, Guid clientId)
        {
            db.Users.Add(new User
            {
                UserId = userId,
                Email = $"user{userId}@test.com",
                FirstName = "Test",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

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
                ChangedFields = Array.Empty<string>(),
                CreatedAt = DateTimeOffset.UtcNow
            });

            db.ClientHistories.Add(new ClientHistory
            {
                HistoryId = Guid.NewGuid(),
                ClientId = clientId,
                ActorUserId = userId,
                ActionType = "UPDATED",
                ChangedFields = new[] { "Email" },
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(5)
            });

            db.SaveChanges();
        }

        [Fact]
        public async Task Self_View_Returns_Own_Activity()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            SeedUserActivity(db, userId, clientId);

            var ctrl = NewController(db, mapper, SalesRep(userId));

            var result = await ctrl.GetUserActivity(userId, null, null, null, 1, 20) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            Assert.True(((int)body.totalCount) >= 2);
        }

        [Fact]
        public async Task Self_View_Other_User_Forbidden()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            SeedUserActivity(db, userId2, clientId);

            var ctrl = NewController(db, mapper, SalesRep(userId1));

            var result = await ctrl.GetUserActivity(userId2, null, null, null, 1, 20);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Admin_Can_View_Any_User_Activity()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            SeedUserActivity(db, userId, clientId);

            var ctrl = NewController(db, mapper, Admin(adminId));

            var result = await ctrl.GetUserActivity(userId, null, null, null, 1, 20) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            Assert.True(((int)body.totalCount) >= 2);
        }

        [Fact]
        public async Task Manager_Can_View_Any_User_Activity()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var managerId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            SeedUserActivity(db, userId, clientId);

            var ctrl = NewController(db, mapper, Manager(managerId));

            var result = await ctrl.GetUserActivity(userId, null, null, null, 1, 20) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
        }

        [Fact]
        public async Task Filter_By_ActionType_Works()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            SeedUserActivity(db, userId, clientId);

            var ctrl = NewController(db, mapper, SalesRep(userId));

            var result = await ctrl.GetUserActivity(userId, "CREATED", null, null, 1, 20) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            Assert.True(((int)body.totalCount) >= 1);
            var data = body.data as System.Collections.IEnumerable;
            Assert.NotNull(data);
            Assert.All(data.Cast<dynamic>(), entry => Assert.Equal("CREATED", (string)entry.actionType));
        }

        [Fact]
        public async Task Filter_By_DateRange_Works()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            SeedUserActivity(db, userId, clientId);

            var ctrl = NewController(db, mapper, SalesRep(userId));

            var from = DateTimeOffset.UtcNow.AddHours(-1);
            var to = DateTimeOffset.UtcNow.AddHours(1);

            var result = await ctrl.GetUserActivity(userId, null, from, to, 1, 20) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            Assert.True(((int)body.totalCount) >= 2);
        }
    }
}

