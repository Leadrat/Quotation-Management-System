using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Clients.Dtos;
using CRM.Application.Clients.Queries;
using CRM.Application.Clients.Queries.Handlers;
using CRM.Application.Mapping;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Clients
{
    public class GetAllClientsQueryTests
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
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new ClientProfile()));
            return new Mapper(config);
        }

        [Fact]
        public async Task SalesRep_Sees_Only_Own_Clients()
        {
            using var db = NewDb();
            var rep1 = Guid.NewGuid();
            var rep2 = Guid.NewGuid();
            db.Users.Add(new User { UserId = rep1, FirstName = "A", LastName = "A" });
            db.Users.Add(new User { UserId = rep2, FirstName = "B", LastName = "B" });
            db.Clients.Add(new Client { ClientId = Guid.NewGuid(), CompanyName = "C1", Email = "c1@x.com", Mobile = "+10000000001", CreatedByUserId = rep1, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
            db.Clients.Add(new Client { ClientId = Guid.NewGuid(), CompanyName = "C2", Email = "c2@x.com", Mobile = "+10000000002", CreatedByUserId = rep2, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
            await db.SaveChangesAsync();

            var handler = new GetAllClientsQueryHandler(db, NewMapper());
            var res = await handler.Handle(new GetAllClientsQuery { RequestorUserId = rep1, RequestorRole = "SalesRep", PageNumber = 1, PageSize = 10 });
            Assert.True(res.Success);
            Assert.All(res.Data, d => Assert.Equal("A A", d.CreatedByUserName));
        }

        [Fact]
        public async Task Pagination_Is_Clamped()
        {
            using var db = NewDb();
            var rep = Guid.NewGuid();
            db.Users.Add(new User { UserId = rep, FirstName = "A", LastName = "A" });
            for (int i = 0; i < 3; i++)
            {
                db.Clients.Add(new Client { ClientId = Guid.NewGuid(), CompanyName = $"C{i}", Email = $"c{i}@x.com", Mobile = "+10000000000", CreatedByUserId = rep, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
            }
            await db.SaveChangesAsync();
            var handler = new GetAllClientsQueryHandler(db, NewMapper());
            var res = await handler.Handle(new GetAllClientsQuery { RequestorUserId = rep, RequestorRole = "SalesRep", PageNumber = 0, PageSize = 1000 });
            Assert.Equal(1, res.PageNumber);
            Assert.Equal(100, res.PageSize);
        }
    }
}
