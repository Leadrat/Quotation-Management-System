using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Mapping;
using CRM.Application.QuotationTemplates.Commands;
using CRM.Application.QuotationTemplates.Commands.Handlers;
using CRM.Application.QuotationTemplates.Exceptions;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.QuotationTemplates
{
    public class ApproveQuotationTemplateCommandHandlerTests
    {
        private static IMapper NewMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new QuotationTemplateProfile()));
            return new Mapper(config);
        }

        [Fact]
        public async Task Handle_ApprovesTemplate()
        {
            using var context = CreateContext();
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            SeedUser(context, userId, "SalesRep");
            SeedUser(context, adminId, "Admin");
            var template = SeedTemplate(context, userId);
            await context.SaveChangesAsync();
            var mapper = NewMapper();

            var handler = new ApproveQuotationTemplateCommandHandler(
                context,
                mapper,
                NullLogger<ApproveQuotationTemplateCommandHandler>.Instance);

            var command = new ApproveQuotationTemplateCommand
            {
                TemplateId = template.TemplateId,
                ApprovedByUserId = adminId
            };

            var result = await handler.Handle(command);

            Assert.True(result.IsApproved);
            Assert.Equal(adminId, result.ApprovedByUserId);
            Assert.NotNull(result.ApprovedAt);

            var approved = await context.QuotationTemplates
                .FirstAsync(t => t.TemplateId == template.TemplateId);
            Assert.True(approved.IsApproved);
        }

        [Fact]
        public async Task Handle_TemplateNotFound_ThrowsException()
        {
            using var context = CreateContext();
            var mapper = NewMapper();
            var handler = new ApproveQuotationTemplateCommandHandler(
                context,
                mapper,
                NullLogger<ApproveQuotationTemplateCommandHandler>.Instance);

            var command = new ApproveQuotationTemplateCommand
            {
                TemplateId = Guid.NewGuid(),
                ApprovedByUserId = Guid.NewGuid()
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
                Email = $"{roleName.ToLower()}@example.com",
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
                Visibility = TemplateVisibility.Public,
                IsApproved = false,
                Version = 1,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.QuotationTemplates.Add(template);
            return template;
        }
    }
}

