using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Clients.Commands;
using CRM.Application.Clients.Commands.Handlers;
using CRM.Application.Clients.Dtos;
using CRM.Application.Clients.Exceptions;
using CRM.Application.Mapping;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Clients
{
    public class CreateClientTests
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
        public async Task Creates_Client_Successfully()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var handler = new CreateClientCommandHandler(db, mapper);
            var dto = await handler.Handle(new CreateClientCommand
            {
                CompanyName = "ABC Corp",
                Email = "Contact@abc.com",
                Mobile = "+11234567890",
                CreatedByUserId = Guid.NewGuid()
            });
            Assert.IsType<ClientDto>(dto);
            Assert.Equal("contact@abc.com", dto.Email);
        }

        [Fact]
        public async Task Duplicate_Email_Throws()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var handler = new CreateClientCommandHandler(db, mapper);
            var owner = Guid.NewGuid();
            await handler.Handle(new CreateClientCommand
            {
                CompanyName = "ABC",
                Email = "dup@x.com",
                Mobile = "+11234567890",
                CreatedByUserId = owner
            });

            await Assert.ThrowsAsync<DuplicateEmailException>(async () =>
            {
                await handler.Handle(new CreateClientCommand
                {
                    CompanyName = "DEF",
                    Email = "DUP@x.com",
                    Mobile = "+11234567890",
                    CreatedByUserId = owner
                });
            });
        }
    }
}
