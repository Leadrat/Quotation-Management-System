using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Mapping;
using CRM.Application.QuotationTemplates.Queries;
using CRM.Application.QuotationTemplates.Queries.Handlers;
using CRM.Application.QuotationTemplates.Exceptions;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.QuotationTemplates
{
    public class GetTemplateByIdQueryHandlerTests
    {
        private static IMapper NewMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new QuotationTemplateProfile()));
            return new Mapper(config);
        }

        [Fact]
        public async Task Handle_ReturnsTemplate()
        {
            using var context = CreateContext();
            var userId = Guid.NewGuid();
            SeedUser(context, userId, "SalesRep");
            var template = SeedTemplate(context, userId);
            await context.SaveChangesAsync();
            var mapper = NewMapper();

            var handler = new GetTemplateByIdQueryHandler(
                context,
                mapper,
                NullLogger<GetTemplateByIdQueryHandler>.Instance);

            var query = new GetTemplateByIdQuery
            {
                TemplateId = template.TemplateId,
                RequestorUserId = userId,
                RequestorRole = "SalesRep"
            };

            var result = await handler.Handle(query);

            Assert.NotNull(result);
            Assert.Equal(template.TemplateId, result.TemplateId);
            Assert.Equal("Test Template", result.Name);
        }

        [Fact]
        public async Task Handle_TemplateNotFound_ThrowsException()
        {
            using var context = CreateContext();
            var mapper = NewMapper();
            var handler = new GetTemplateByIdQueryHandler(
                context,
                mapper,
                NullLogger<GetTemplateByIdQueryHandler>.Instance);

            var query = new GetTemplateByIdQuery
            {
                TemplateId = Guid.NewGuid(),
                RequestorUserId = Guid.NewGuid(),
                RequestorRole = "SalesRep"
            };

            await Assert.ThrowsAsync<QuotationTemplateNotFoundException>(() => handler.Handle(query));
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

        private static QuotationTemplate SeedTemplate(AppDbContext context, Guid ownerId)
        {
            var template = new QuotationTemplate
            {
                TemplateId = Guid.NewGuid(),
                Name = "Test Template",
                OwnerUserId = ownerId,
                OwnerRole = "SalesRep",
                Visibility = TemplateVisibility.Private,
                Version = 1,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.QuotationTemplates.Add(template);
            return template;
        }
    }
}

