using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Mapping;
using CRM.Application.QuotationTemplates.Commands;
using CRM.Application.QuotationTemplates.Commands.Handlers;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Application.QuotationTemplates.Exceptions;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.QuotationTemplates
{
    public class UpdateQuotationTemplateCommandHandlerTests
    {
        private static IMapper NewMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new QuotationTemplateProfile()));
            return new Mapper(config);
        }

        [Fact]
        public async Task Handle_UpdatesTemplateAndCreatesVersion()
        {
            using var context = CreateContext();
            var userId = Guid.NewGuid();
            SeedUser(context, userId, "SalesRep");
            var template = SeedTemplate(context, userId, "Original Name", 1);
            await context.SaveChangesAsync();
            var mapper = NewMapper();

            var handler = new UpdateQuotationTemplateCommandHandler(
                context,
                mapper,
                NullLogger<UpdateQuotationTemplateCommandHandler>.Instance);

            var command = new UpdateQuotationTemplateCommand
            {
                TemplateId = template.TemplateId,
                UpdatedByUserId = userId,
                RequestorRole = "SalesRep",
                Request = new UpdateQuotationTemplateRequest
                {
                    Name = "Updated Name",
                    Visibility = "Public"
                }
            };

            var result = await handler.Handle(command);

            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("Public", result.Visibility);
            Assert.Equal(2, result.Version);

            var updated = await context.QuotationTemplates
                .FirstAsync(t => t.TemplateId == template.TemplateId);
            Assert.Equal("Updated Name", updated.Name);
            Assert.Equal(2, updated.Version);
        }

        [Fact]
        public async Task Handle_TemplateNotFound_ThrowsException()
        {
            using var context = CreateContext();
            var mapper = NewMapper();
            var handler = new UpdateQuotationTemplateCommandHandler(
                context,
                mapper,
                NullLogger<UpdateQuotationTemplateCommandHandler>.Instance);

            var command = new UpdateQuotationTemplateCommand
            {
                TemplateId = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                RequestorRole = "SalesRep",
                Request = new UpdateQuotationTemplateRequest()
            };

            await Assert.ThrowsAsync<QuotationTemplateNotFoundException>(() => handler.Handle(command));
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

        private static QuotationTemplate SeedTemplate(AppDbContext context, Guid ownerId, string name, int version)
        {
            var template = new QuotationTemplate
            {
                TemplateId = Guid.NewGuid(),
                Name = name,
                OwnerUserId = ownerId,
                OwnerRole = "SalesRep",
                Visibility = TemplateVisibility.Private,
                Version = version,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.QuotationTemplates.Add(template);
            return template;
        }
    }
}

