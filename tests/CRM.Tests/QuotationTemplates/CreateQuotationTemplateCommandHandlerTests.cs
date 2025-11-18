using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Mapping;
using CRM.Application.QuotationTemplates.Commands;
using CRM.Application.QuotationTemplates.Commands.Handlers;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.QuotationTemplates
{
    public class CreateQuotationTemplateCommandHandlerTests
    {
        private static IMapper NewMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new QuotationTemplateProfile()));
            return new Mapper(config);
        }

        [Fact]
        public async Task Handle_CreatesTemplateSuccessfully()
        {
            using var context = CreateContext();
            var userId = Guid.NewGuid();
            SeedUser(context, userId, "SalesRep");
            var mapper = NewMapper();

            var handler = new CreateQuotationTemplateCommandHandler(
                context,
                mapper,
                NullLogger<CreateQuotationTemplateCommandHandler>.Instance);

            var command = new CreateQuotationTemplateCommand
            {
                CreatedByUserId = userId,
                Request = new CreateQuotationTemplateRequest
                {
                    Name = "Test Template",
                    Description = "Test Description",
                    Visibility = "Private",
                    LineItems = new List<CreateTemplateLineItemRequest>
                    {
                        new CreateTemplateLineItemRequest
                        {
                            ItemName = "Item 1",
                            Quantity = 1,
                            UnitRate = 100
                        }
                    }
                }
            };

            var result = await handler.Handle(command);

            Assert.NotNull(result);
            Assert.Equal("Test Template", result.Name);
            Assert.Equal("Private", result.Visibility);

            var saved = await context.QuotationTemplates
                .Include(t => t.LineItems)
                .FirstAsync(t => t.TemplateId == result.TemplateId);

            Assert.NotNull(saved);
            Assert.Single(saved.LineItems);
            Assert.Equal(1, saved.Version);
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static void SeedUser(AppDbContext context, Guid userId, string roleName)
        {
            var role = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = roleName
            };
            context.Roles.Add(role);

            context.Users.Add(new User
            {
                UserId = userId,
                Email = "user@example.com",
                FirstName = "Test",
                LastName = "User",
                RoleId = role.RoleId,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = "hash"
            });
        }
    }
}

