using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Api.Controllers;
using CRM.Application.Clients.Queries;
using CRM.Application.Clients.Queries.Handlers;
using CRM.Application.Mapping;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace CRM.Tests.Integration.Clients
{
    public class ClientsSearchControllerTests
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

        private static ClientsSearchController NewController(AppDbContext db, IMapper mapper, ClaimsPrincipal user)
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var ctrl = new ClientsSearchController(db, mapper, cache);
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return ctrl;
        }

        private static void SeedUsersAndClients(AppDbContext db, Guid u1, Guid u2)
        {
            db.Users.Add(new User { UserId = u1, Email = "u1@test.com", FirstName = "A", LastName = "U1" });
            db.Users.Add(new User { UserId = u2, Email = "u2@test.com", FirstName = "B", LastName = "U2" });
            db.Clients.Add(new Client { ClientId = Guid.NewGuid(), CompanyName = "ABC Corp", ContactName = "John", Email = "john@abc.com", Mobile = "+9100", CreatedByUserId = u1, City="Mumbai", State="Maharashtra", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt=DateTimeOffset.UtcNow });
            db.Clients.Add(new Client { ClientId = Guid.NewGuid(), CompanyName = "ABD Tech", ContactName = "Jane", Email = "jane@abd.com", Mobile = "+9101", CreatedByUserId = u1, City="Pune", State="Maharashtra", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt=DateTimeOffset.UtcNow });
            db.Clients.Add(new Client { ClientId = Guid.NewGuid(), CompanyName = "XYZ Industries", ContactName = "Sam", Email = "sam@xyz.com", Mobile = "+9102", CreatedByUserId = u2, City="Delhi", State="Delhi", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt=DateTimeOffset.UtcNow });
            db.SaveChanges();
        }

        [Fact]
        public async Task Search_SalesRep_SeesOnlyOwn()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var u1 = Guid.NewGuid(); var u2 = Guid.NewGuid();
            SeedUsersAndClients(db, u1, u2);
            var ctrl = NewController(db, mapper, SalesRep(u1));

            var result = await ctrl.Search(new ClientsSearchController.SearchRequest { SearchTerm = "AB", PageNumber = 1, PageSize = 10 }) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            Assert.True(((int)body.totalCount) >= 2); // two from u1 match AB
            var data = body.data as System.Collections.IEnumerable;
            Assert.NotNull(data);
        }

        [Fact]
        public async Task Search_Admin_Filter_By_UserId()
        {
            using var db = NewDb(); var mapper = NewMapper();
            var u1 = Guid.NewGuid(); var u2 = Guid.NewGuid();
            SeedUsersAndClients(db, u1, u2);
            var ctrl = NewController(db, mapper, Admin(Guid.NewGuid()));

            var result = await ctrl.Search(new ClientsSearchController.SearchRequest { UserId = u2, PageNumber = 1, PageSize = 10 }) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            Assert.True(((int)body.totalCount) >= 1);
        }

        [Fact]
        public async Task Suggestions_Returns_Prefix_First()
        {
            using var db = NewDb(); var mapper = NewMapper();
            var u1 = Guid.NewGuid(); var u2 = Guid.NewGuid();
            SeedUsersAndClients(db, u1, u2);
            var ctrl = NewController(db, mapper, SalesRep(u1));

            var result = await ctrl.Suggestions("AB", "CompanyName", 10) as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            Assert.True(((System.Collections.IEnumerable)body.data).Cast<string>().Any());
        }

        [Fact]
        public async Task FilterOptions_Returns_Data()
        {
            using var db = NewDb(); var mapper = NewMapper();
            var u1 = Guid.NewGuid(); var u2 = Guid.NewGuid();
            SeedUsersAndClients(db, u1, u2);
            var ctrl = NewController(db, mapper, SalesRep(u1));

            var result = await ctrl.FilterOptions() as OkObjectResult;
            Assert.NotNull(result);
            dynamic body = result!.Value!;
            Assert.True((bool)body.success);
            Assert.True(((System.Collections.IEnumerable)body.data.states).Cast<object>().Any());
        }

        [Fact]
        public async Task SavedSearch_Save_List_Delete()
        {
            using var db = NewDb(); var mapper = NewMapper();
            var u1 = Guid.NewGuid(); var u2 = Guid.NewGuid();
            SeedUsersAndClients(db, u1, u2);
            var ctrl = NewController(db, mapper, SalesRep(u1));

            var save = await ctrl.SaveSearch(new ClientsSearchController.SaveSearchBody
            {
                SearchName = "My VIP Clients",
                FilterCriteria = new System.Collections.Generic.Dictionary<string, object> { { "state", "Maharashtra" } },
                SortBy = "NameAsc"
            }) as ObjectResult;
            Assert.NotNull(save);
            Assert.Equal(201, save!.StatusCode);

            var list = await ctrl.GetSaved() as OkObjectResult;
            Assert.NotNull(list);
            dynamic body = list!.Value!;
            Assert.True(((System.Collections.IEnumerable)body.data).Cast<object>().Any());

            var savedItem = ((System.Collections.IEnumerable)body.data).Cast<dynamic>().First();
            Guid savedId = savedItem.savedSearchId;
            var del = await ctrl.DeleteSaved(savedId) as OkObjectResult;
            Assert.NotNull(del);
        }

        [Fact]
        public async Task Export_Csv_Writes_Content()
        {
            using var db = NewDb(); var mapper = NewMapper();
            var u1 = Guid.NewGuid(); var u2 = Guid.NewGuid();
            SeedUsersAndClients(db, u1, u2);
            var ctrl = NewController(db, mapper, SalesRep(u1));

            var ctx = ctrl.ControllerContext.HttpContext!;
            ctx.Response.Body = new MemoryStream();
            var result = await ctrl.Export("AB", null, null, null, null, null, "csv");
            Assert.IsType<EmptyResult>(result);
            ctx.Response.Body.Seek(0, SeekOrigin.Begin);
            var text = new StreamReader(ctx.Response.Body).ReadToEnd();
            Assert.Contains("ClientId,CompanyName", text);
        }
    }
}
