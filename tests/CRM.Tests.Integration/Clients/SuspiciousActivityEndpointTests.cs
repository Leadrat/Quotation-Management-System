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
    public class SuspiciousActivityEndpointTests
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

        private static ClaimsPrincipal Admin(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            }, "TestAuth"));
        }

        private static ClaimsPrincipal SalesRep(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim(ClaimTypes.Role, "SalesRep")
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

        private static void SeedSuspiciousActivity(AppDbContext db, Guid userId, Guid clientId, Guid historyId)
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
                HistoryId = historyId,
                ClientId = clientId,
                ActorUserId = userId,
                ActionType = "UPDATED",
                ChangedFields = new[] { "Email", "Mobile", "Address" },
                SuspicionScore = 8,
                CreatedAt = DateTimeOffset.UtcNow
            });

            db.SuspiciousActivityFlags.Add(new SuspiciousActivityFlag
            {
                FlagId = Guid.NewGuid(),
                HistoryId = historyId,
                ClientId = clientId,
                Score = 8,
                Reasons = new[] { "Rapid changes: 6 changes in last hour", "Unusual time: activity at 2:00" },
                DetectedAt = DateTimeOffset.UtcNow,
                Status = "OPEN",
                Metadata = "{}"
            });

            db.SaveChanges();
        }

        [Fact]
        public async Task Admin_Can_View_Suspicious_Activity()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var historyId = Guid.NewGuid();
            SeedSuspiciousActivity(db, userId, clientId, historyId);

            var ctrl = NewController(db, mapper, Admin(adminId));

            var result = await ctrl.GetSuspiciousActivity(7, null, null, null, 1, 20) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            Assert.True(((int)body.totalCount) >= 1);
        }

        [Fact]
        public async Task SalesRep_Cannot_View_Suspicious_Activity()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var repId = Guid.NewGuid();

            var ctrl = NewController(db, mapper, SalesRep(repId));

            var result = await ctrl.GetSuspiciousActivity(7, null, null, null, 1, 20);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Filter_By_MinScore_Works()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var historyId = Guid.NewGuid();
            SeedSuspiciousActivity(db, userId, clientId, historyId);

            var ctrl = NewController(db, mapper, Admin(adminId));

            var result = await ctrl.GetSuspiciousActivity(9, null, null, null, 1, 20) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            // Should return 0 results since our flag has score 8
            Assert.Equal(0, (int)body.totalCount);
        }

        [Fact]
        public async Task Filter_By_Status_Works()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var historyId = Guid.NewGuid();
            SeedSuspiciousActivity(db, userId, clientId, historyId);

            var ctrl = NewController(db, mapper, Admin(adminId));

            var result = await ctrl.GetSuspiciousActivity(7, "OPEN", null, null, 1, 20) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            Assert.True(((int)body.totalCount) >= 1);
        }
    }
}

