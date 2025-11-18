using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Quotations.Commands;
using CRM.Application.Quotations.Commands.Handlers;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Exceptions;
using CRM.Application.Quotations.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class SendQuotationCommandHandlerTests
    {
        [Fact]
        public async Task Handle_SendsQuotationAndCreatesAccessLink()
        {
            using var context = CreateContext();
            var userId = Guid.NewGuid();
            SeedUserWithRole(context, userId, "SalesRep");
            var quotation = SeedQuotation(context, userId, QuotationStatus.Draft);
            await context.SaveChangesAsync();

            var workflow = new TestWorkflow();
            var handler = CreateHandler(context, workflow);

            var command = new SendQuotationCommand
            {
                QuotationId = quotation.QuotationId,
                RequestedByUserId = userId,
                Request = new SendQuotationRequest
                {
                    RecipientEmail = "client@example.com"
                }
            };

            var result = await handler.Handle(command);

            Assert.NotNull(result);
            Assert.True(workflow.Executed);
            Assert.False(workflow.IsResend);

            var savedQuotation = await context.Quotations.FirstAsync(q => q.QuotationId == quotation.QuotationId);
            Assert.Equal(QuotationStatus.Sent, savedQuotation.Status);
        }

        [Fact]
        public async Task Handle_NonDraftQuotation_ThrowsInvalidStatus()
        {
            using var context = CreateContext();
            var userId = Guid.NewGuid();
            SeedUserWithRole(context, userId, "SalesRep");
            var quotation = SeedQuotation(context, userId, QuotationStatus.Viewed);
            await context.SaveChangesAsync();

            var handler = CreateHandler(context, new TestWorkflow());

            var command = new SendQuotationCommand
            {
                QuotationId = quotation.QuotationId,
                RequestedByUserId = userId,
                Request = new SendQuotationRequest
                {
                    RecipientEmail = "client@example.com"
                }
            };

            await Assert.ThrowsAsync<InvalidQuotationStatusException>(() => handler.Handle(command));
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static SendQuotationCommandHandler CreateHandler(
            AppDbContext context,
            IQuotationSendWorkflow workflow)
        {
            return new SendQuotationCommandHandler(
                context,
                workflow,
                NullLogger<SendQuotationCommandHandler>.Instance);
        }

        private static void SeedUserWithRole(AppDbContext context, Guid userId, string roleName)
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
                Email = "sales@example.com",
                FirstName = "Sales",
                LastName = "Rep",
                RoleId = role.RoleId,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = "hash"
            });
        }

        private static Quotation SeedQuotation(AppDbContext context, Guid createdByUserId, QuotationStatus status)
        {
            var client = new Client
            {
                ClientId = Guid.NewGuid(),
                CompanyName = "Client Co",
                Email = "client@example.com",
                StateCode = "27",
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            context.Clients.Add(client);

            var quotation = new Quotation
            {
                QuotationId = Guid.NewGuid(),
                ClientId = client.ClientId,
                Client = client,
                CreatedByUserId = createdByUserId,
                QuotationNumber = "QT-001",
                Status = status,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                SubTotal = 100,
                DiscountAmount = 0,
                DiscountPercentage = 0,
                TaxAmount = 0,
                TotalAmount = 100,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            quotation.LineItems.Add(new QuotationLineItem
            {
                LineItemId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                Quotation = quotation,
                SequenceNumber = 1,
                ItemName = "Item 1",
                Quantity = 1,
                UnitRate = 100,
                Amount = 100,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            context.Quotations.Add(quotation);

            return quotation;
        }

        private class TestWorkflow : IQuotationSendWorkflow
        {
            public bool Executed { get; private set; }
            public bool IsResend { get; private set; }

            public Task<QuotationAccessLinkDto> ExecuteAsync(Quotation quotation, SendQuotationRequest request, Guid requestedByUserId, bool isResend)
            {
                Executed = true;
                IsResend = isResend;
                quotation.Status = QuotationStatus.Sent;
                return Task.FromResult(new QuotationAccessLinkDto
                {
                    AccessLinkId = Guid.NewGuid(),
                    QuotationId = quotation.QuotationId,
                    ClientEmail = request.RecipientEmail,
                    ViewUrl = "https://app.test",
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }
    }
}


