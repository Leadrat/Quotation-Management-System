using System;
using System.Collections.Generic;
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
    public class SubmitQuotationResponseCommandHandlerTests
    {
        [Fact]
        public async Task Handle_AcceptedResponse_UpdatesStatusAndSendsNotification()
        {
            using var context = CreateContext();
            var (quotation, link) = await SeedQuotationWithLink(context, QuotationStatus.Sent);

            var emailService = new TestEmailService();
            var handler = new SubmitQuotationResponseCommandHandler(
                context,
                emailService,
                NullLogger<SubmitQuotationResponseCommandHandler>.Instance);

            var command = new SubmitQuotationResponseCommand
            {
                AccessToken = link.AccessToken,
                Request = new SubmitQuotationResponseRequest
                {
                    ResponseType = "ACCEPTED",
                    ClientEmail = link.ClientEmail,
                    ClientName = "Client"
                }
            };

            var result = await handler.Handle(command);

            Assert.Equal("ACCEPTED", result.ResponseType);
            Assert.True(emailService.AcceptedSent);

            var updatedQuotation = await context.Quotations.FirstAsync(q => q.QuotationId == quotation.QuotationId);
            Assert.Equal(QuotationStatus.Accepted, updatedQuotation.Status);
        }

        [Fact]
        public async Task Handle_DuplicateResponse_Throws()
        {
            using var context = CreateContext();
            var (quotation, link) = await SeedQuotationWithLink(context, QuotationStatus.Sent);

            context.QuotationResponses.Add(new QuotationResponse
            {
                ResponseId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                ResponseType = "ACCEPTED",
                ClientEmail = link.ClientEmail,
                ResponseDate = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();

            var handler = new SubmitQuotationResponseCommandHandler(
                context,
                new TestEmailService(),
                NullLogger<SubmitQuotationResponseCommandHandler>.Instance);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new SubmitQuotationResponseCommand
                {
                    AccessToken = link.AccessToken,
                    Request = new SubmitQuotationResponseRequest
                    {
                        ResponseType = "ACCEPTED"
                    }
                }));
        }

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static async Task<(Quotation, QuotationAccessLink)> SeedQuotationWithLink(AppDbContext context, QuotationStatus status)
        {
            var role = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = "SalesRep"
            };
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
                CompanyName = "Client Co",
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
            context.Quotations.Add(quotation);

            var link = new QuotationAccessLink
            {
                AccessLinkId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                Quotation = quotation,
                ClientEmail = "client@example.com",
                AccessToken = "token-" + Guid.NewGuid(),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
            };
            context.QuotationAccessLinks.Add(link);

            await context.SaveChangesAsync();

            return (quotation, link);
        }

        private class TestEmailService : IQuotationEmailService
        {
            public bool AcceptedSent { get; private set; }

            public Task SendQuotationEmailAsync(Quotation quotation, string recipientEmail, byte[] pdfAttachment, string accessLink, System.Collections.Generic.List<string>? ccEmails = null, System.Collections.Generic.List<string>? bccEmails = null, string? customMessage = null)
            {
                return Task.CompletedTask;
            }

            public Task SendQuotationAcceptedNotificationAsync(Quotation quotation, QuotationResponse response, string salesRepEmail)
            {
                AcceptedSent = true;
                return Task.CompletedTask;
            }

            public Task SendQuotationRejectedNotificationAsync(Quotation quotation, QuotationResponse response, string salesRepEmail)
            {
                return Task.CompletedTask;
            }

            public Task SendUnviewedQuotationReminderAsync(Quotation quotation, string salesRepEmail, DateTimeOffset sentAt)
            {
                return Task.CompletedTask;
            }

            public Task SendPendingResponseFollowUpAsync(Quotation quotation, string salesRepEmail, DateTimeOffset firstViewedAt)
            {
                return Task.CompletedTask;
            }
        }
    }
}


