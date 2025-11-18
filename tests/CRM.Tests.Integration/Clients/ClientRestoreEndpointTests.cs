using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Api.Controllers;
using CRM.Application.Clients.Commands;
using CRM.Application.Clients.Commands.Handlers;
using CRM.Application.Clients.Validators;
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
    public class ClientRestoreEndpointTests
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

        private static ClientsHistoryController NewController(AppDbContext db, IMapper mapper, ClaimsPrincipal user)
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var controller = new ClientsHistoryController(db, mapper, cache, Options.Create(new HistorySettings()));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            controller.ControllerContext.HttpContext!.Request.Headers["User-Agent"] = "UnitTest";
            return controller;
        }

        private static ClaimsPrincipal Admin(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("role", "Admin")
            }, "TestAuth"));
        }

        private static (Guid clientId, Guid adminId) SeedDeletedClient(AppDbContext db, bool expired = false)
        {
            var roleId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            db.Roles.Add(new Role { RoleId = roleId, RoleName = "Admin", IsActive = true, CreatedAt = now.UtcDateTime, UpdatedAt = now.UtcDateTime });
            db.Users.Add(new User
            {
                UserId = adminId,
                Email = "admin@test.com",
                FirstName = "Admin",
                LastName = "User",
                RoleId = roleId,
                CreatedAt = now.UtcDateTime,
                UpdatedAt = now.UtcDateTime,
                IsActive = true
            });
            db.Clients.Add(new Client
            {
                ClientId = clientId,
                CompanyName = "Deleted Co",
                Email = "deleted@co.com",
                Mobile = "+911234567890",
                CreatedByUserId = adminId,
                CreatedAt = now.AddDays(-40),
                UpdatedAt = now.AddDays(-35),
                DeletedAt = expired ? now.AddDays(-40) : now.AddDays(-5)
            });
            db.SaveChanges();
            return (clientId, adminId);
        }

        [Fact]
        public async Task Restore_Succeeds_For_Admin_WithIn_Window()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (clientId, adminId) = SeedDeletedClient(db);
            var controller = NewController(db, mapper, Admin(adminId));

            var result = await controller.Restore(clientId, new ClientsHistoryController.RestoreClientBody { Reason = "Accidental delete" }) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            Assert.Equal("Client restored successfully", (string)body.message);
        }

        [Fact]
        public async Task Restore_Fails_When_Window_Expired()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (clientId, adminId) = SeedDeletedClient(db, expired: true);
            var controller = NewController(db, mapper, Admin(adminId));

            var result = await controller.Restore(clientId, new ClientsHistoryController.RestoreClientBody { Reason = "Late attempt" }) as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result!.StatusCode);
        }
    }
}

