using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
    public class ClientHistoryExportTests
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

        private static ClaimsPrincipal Manager(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim(ClaimTypes.Role, "Manager")
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

        private static void SeedHistoryData(AppDbContext db, Guid userId, Guid clientId)
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
        public async Task Admin_Can_Export_History_As_Csv()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            SeedHistoryData(db, userId, clientId);

            var ctrl = NewController(db, mapper, Admin(adminId));

            var result = await ctrl.ExportHistory(clientId.ToString(), null, null, null, "csv") as FileStreamResult;
            Assert.NotNull(result);
            Assert.Equal("text/csv", result!.ContentType);
            Assert.Contains("client_history_", result.FileDownloadName);

            var stream = new MemoryStream();
            await result.FileStream.CopyToAsync(stream);
            stream.Position = 0;
            var text = Encoding.UTF8.GetString(stream.ToArray());
            Assert.Contains("HistoryId,ClientId,ActionType", text);
        }

        [Fact]
        public async Task Manager_Can_Export_History()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var managerId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            SeedHistoryData(db, userId, clientId);

            var ctrl = NewController(db, mapper, Manager(managerId));

            var result = await ctrl.ExportHistory(clientId.ToString(), null, null, null, "csv") as FileStreamResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SalesRep_Cannot_Export_History()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var repId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            SeedHistoryData(db, userId, clientId);

            var ctrl = NewController(db, mapper, SalesRep(repId));

            var result = await ctrl.ExportHistory(clientId.ToString(), null, null, null, "csv");
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Pdf_Format_Returns_Placeholder_Message()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            SeedHistoryData(db, userId, clientId);

            var ctrl = NewController(db, mapper, Admin(adminId));

            var result = await ctrl.ExportHistory(clientId.ToString(), null, null, null, "pdf") as BadRequestObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.Contains("PDF export not available", (string)body.error);
        }

        [Fact]
        public async Task Empty_Results_Returns_Valid_Csv_With_Headers()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var adminId = Guid.NewGuid();

            var ctrl = NewController(db, mapper, Admin(adminId));

            var result = await ctrl.ExportHistory(Guid.NewGuid().ToString(), null, null, null, "csv") as FileStreamResult;
            Assert.NotNull(result);

            var stream = new MemoryStream();
            await result!.FileStream.CopyToAsync(stream);
            stream.Position = 0;
            var text = Encoding.UTF8.GetString(stream.ToArray());
            Assert.Contains("HistoryId,ClientId,ActionType", text);
        }
    }
}

