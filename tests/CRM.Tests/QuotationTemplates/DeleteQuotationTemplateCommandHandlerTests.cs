using System;
using System.Threading.Tasks;
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
    public class DeleteQuotationTemplateCommandHandlerTests
    {
        [Fact]
        public async Task Handle_SoftDeletesTemplate()
        {
            using var context = CreateContext();
            var userId = Guid.NewGuid();
            SeedUser(context, userId, "SalesRep");
            var template = SeedTemplate(context, userId);
            await context.SaveChangesAsync();

            var handler = new DeleteQuotationTemplateCommandHandler(
                context,
                NullLogger<DeleteQuotationTemplateCommandHandler>.Instance);

            var command = new DeleteQuotationTemplateCommand
            {
                TemplateId = template.TemplateId,
                DeletedByUserId = userId,
                RequestorRole = "SalesRep"
            };

            await handler.Handle(command);

            var deleted = await context.QuotationTemplates
                .FirstAsync(t => t.TemplateId == template.TemplateId);
            Assert.NotNull(deleted.DeletedAt);
        }

        [Fact]
        public async Task Handle_TemplateNotFound_ThrowsException()
        {
            using var context = CreateContext();
            var handler = new DeleteQuotationTemplateCommandHandler(
                context,
                NullLogger<DeleteQuotationTemplateCommandHandler>.Instance);

            var command = new DeleteQuotationTemplateCommand
            {
                TemplateId = Guid.NewGuid(),
                DeletedByUserId = Guid.NewGuid(),
                RequestorRole = "SalesRep"
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

