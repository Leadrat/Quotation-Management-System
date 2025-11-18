using System;
using System.Threading.Tasks;
using CRM.Application.Quotations.Commands;
using CRM.Application.Quotations.Commands.Handlers;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class ResendQuotationCommandHandlerTests
    {
        [Fact]
        public async Task Handle_AllowsResendForSentQuotation()
        {
            using var context = CreateContext();
            var (quotation, userId) = await SeedQuotation(context, QuotationStatus.Sent);

            var workflow = new TestWorkflow();
            var handler = new ResendQuotationCommandHandler(context, workflow, NullLogger<ResendQuotationCommandHandler>.Instance);

            var dto = await handler.Handle(new ResendQuotationCommand
            {
                QuotationId = quotation.QuotationId,
                RequestedByUserId = userId,
                Request = new SendQuotationRequest { RecipientEmail = "client@example.com" }
            });

            Assert.True(workflow.Executed);
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static async Task<(Quotation, Guid)> SeedQuotation(AppDbContext context, QuotationStatus status)
        {
            var role = new Role { RoleId = Guid.NewGuid(), RoleName = "SalesRep" };
            context.Roles.Add(role);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = "sales@example.com",
                FirstName = "Sales",
                LastName = "Rep",
                RoleId = role.RoleId,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = "hash"
            };
            context.Users.Add(user);

            var client = new Client
            {
                ClientId = Guid.NewGuid(),
                CompanyName = "Client",
                Email = "client@example.com",
                StateCode = "27",
                CreatedByUserId = user.UserId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.Clients.Add(client);

            var quotation = new Quotation
            {
                QuotationId = Guid.NewGuid(),
                ClientId = client.ClientId,
                Client = client,
                CreatedByUserId = user.UserId,
                CreatedByUser = user,
                QuotationNumber = "QT-001",
                Status = status,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                SubTotal = 100,
                TotalAmount = 100,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            quotation.LineItems.Add(new QuotationLineItem
            {
                LineItemId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                ItemName = "Item",
                Quantity = 1,
                UnitRate = 100,
                Amount = 100,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            context.Quotations.Add(quotation);
            await context.SaveChangesAsync();

            return (quotation, user.UserId);
        }

        private class TestWorkflow : IQuotationSendWorkflow
        {
            public bool Executed { get; private set; }

            public Task<QuotationAccessLinkDto> ExecuteAsync(Quotation quotation, SendQuotationRequest request, Guid requestedByUserId, bool isResend)
            {
                Executed = true;
                return Task.FromResult(new QuotationAccessLinkDto
                {
                    AccessLinkId = Guid.NewGuid(),
                    QuotationId = quotation.QuotationId,
                    ClientEmail = request.RecipientEmail,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }
    }
}


