using System;
using System.Collections.Generic;
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
    public class ClientHistoryEndpointTests
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
                c.AddProfile(new ClientProfile());
                c.AddProfile(new ClientHistoryProfile());
                c.AddProfile(new UserProfile());
            });
            return cfg.CreateMapper();
        }

        private static ClaimsPrincipal SalesRep(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("role", "SalesRep")
            }, "TestAuth"));
        }

        private static ClaimsPrincipal Admin(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("role", "Admin")
            }, "TestAuth"));
        }

        private static ClientsHistoryController NewController(AppDbContext db, IMapper mapper, ClaimsPrincipal user)
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var ctrl = new ClientsHistoryController(db, mapper, cache, Options.Create(new HistorySettings()));
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return ctrl;
        }

        private static (Guid clientId, Guid ownerId) SeedHistory(AppDbContext db)
        {
            var ownerId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var roleId = Guid.NewGuid();

            db.Roles.Add(new Role
            {
                RoleId = roleId,
                RoleName = "TestRole",
                IsActive = true
            });

            db.Users.Add(new User { UserId = ownerId, Email = "owner@test.com", FirstName = "Owner", LastName = "One", RoleId = roleId, CreatedAt = now, UpdatedAt = now, IsActive = true });
            db.Users.Add(new User { UserId = adminId, Email = "admin@test.com", FirstName = "Admin", LastName = "One", RoleId = roleId, CreatedAt = now, UpdatedAt = now, IsActive = true });

            db.Clients.Add(new Client
            {
                ClientId = clientId,
                CompanyName = "History Corp",
                Email = "hc@example.com",
                Mobile = "+91123456789",
                CreatedByUserId = ownerId,
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-1)
            });

            db.ClientHistories.AddRange(
                new ClientHistory
                {
                    HistoryId = Guid.NewGuid(),
                    ClientId = clientId,
                    ActorUserId = ownerId,
                    ActionType = "CREATED",
                    ChangedFields = new List<string> { "CompanyName" },
                    AfterSnapshot = "{\"companyName\":\"History Corp\"}",
                    Metadata = "{}",
                    CreatedAt = now.AddDays(-2)
                },
                new ClientHistory
                {
                    HistoryId = Guid.NewGuid(),
                    ClientId = clientId,
                    ActorUserId = ownerId,
                    ActionType = "ACCESSED",
                    ChangedFields = new List<string>(),
                    Metadata = "{}",
                    CreatedAt = now.AddHours(-3)
                });

            db.SaveChanges();
            return (clientId, ownerId);
        }

        [Fact]
        public async Task Owner_History_Excludes_Access_Logs_By_Default()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (clientId, ownerId) = SeedHistory(db);

            var ctrl = NewController(db, mapper, SalesRep(ownerId));
            var result = await ctrl.GetHistory(clientId) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            foreach (var entry in body.data)
            {
                Assert.NotEqual("ACCESSED", (string)entry.actionType);
            }
        }

        [Fact]
        public async Task Admin_History_Includes_Access_Logs_When_Requested()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (clientId, _) = SeedHistory(db);

            var ctrl = NewController(db, mapper, Admin(Guid.NewGuid()));
            var result = await ctrl.GetHistory(clientId, includeAccessLogs: true) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);

            bool hasAccessed = false;
            foreach (var entry in body.data)
            {
                if ((string)entry.actionType == "ACCESSED") hasAccessed = true;
            }
            Assert.True(hasAccessed);
        }

        [Fact]
        public async Task Timeline_Returns_Summary_With_Last_Modified()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (clientId, ownerId) = SeedHistory(db);

            var ctrl = NewController(db, mapper, SalesRep(ownerId));
            var result = await ctrl.GetTimeline(clientId) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            Assert.Equal("History Corp", (string)body.data.companyName);
            Assert.NotNull(body.data.lastModifiedAt);
        }
    }
}

