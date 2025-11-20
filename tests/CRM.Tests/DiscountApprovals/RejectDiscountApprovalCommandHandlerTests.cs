using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.DiscountApprovals.Commands;
using CRM.Application.DiscountApprovals.Commands.Handlers;
using CRM.Application.DiscountApprovals.Dtos;
using CRM.Application.DiscountApprovals.Exceptions;
using CRM.Application.Mapping;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.DiscountApprovals
{
    public class RejectDiscountApprovalCommandHandlerTests
    {
        private static IMapper NewMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new DiscountApprovalProfile()));
            return new Mapper(config);
        }

        [Fact]
        public async Task Handle_RejectsPendingApproval_AndUnlocksQuotation()
        {
            using var context = CreateContext();
            var (approval, approverUserId) = await SeedPendingApproval(context);
            var mapper = NewMapper();

            var handler = new RejectDiscountApprovalCommandHandler(
                context,
                mapper,
                NullLogger<RejectDiscountApprovalCommandHandler>.Instance);

            var command = new RejectDiscountApprovalCommand
            {
                ApprovalId = approval.ApprovalId,
                RejectedByUserId = approverUserId,
                Request = new RejectDiscountApprovalRequest
                {
                    Reason = "Discount too high for this client segment"
                }
            };

            var result = await handler.Handle(command);

            Assert.NotNull(result);
            Assert.Equal(ApprovalStatus.Rejected.ToString(), result.Status);
            Assert.NotNull(result.RejectionDate);

            var saved = await context.DiscountApprovals
                .FirstAsync(a => a.ApprovalId == approval.ApprovalId);

            Assert.Equal(ApprovalStatus.Rejected, saved.Status);
            Assert.NotNull(saved.RejectionDate);
            Assert.Equal(approverUserId, saved.ApproverUserId);

            var quotation = await context.Quotations
                .FirstAsync(q => q.QuotationId == approval.QuotationId);

            Assert.False(quotation.IsPendingApproval);
            Assert.Null(quotation.PendingApprovalId);
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenApprovalNotFound()
        {
            using var context = CreateContext();
            var approverUserId = Guid.NewGuid();
            SeedUser(context, approverUserId, "Manager");
            var mapper = NewMapper();

            var handler = new RejectDiscountApprovalCommandHandler(
                context,
                mapper,
                NullLogger<RejectDiscountApprovalCommandHandler>.Instance);

            var command = new RejectDiscountApprovalCommand
            {
                ApprovalId = Guid.NewGuid(),
                RejectedByUserId = approverUserId,
                Request = new RejectDiscountApprovalRequest
                {
                    Reason = "Test reason"
                }
            };

            await Assert.ThrowsAsync<DiscountApprovalNotFoundException>(() => handler.Handle(command));
        }

        [Fact]
        public async Task Handle_ThrowsException_WhenApprovalNotPending()
        {
            using var context = CreateContext();
            var (approval, approverUserId) = await SeedRejectedApproval(context);
            var mapper = NewMapper();

            var handler = new RejectDiscountApprovalCommandHandler(
                context,
                mapper,
                NullLogger<RejectDiscountApprovalCommandHandler>.Instance);

            var command = new RejectDiscountApprovalCommand
            {
                ApprovalId = approval.ApprovalId,
                RejectedByUserId = approverUserId,
                Request = new RejectDiscountApprovalRequest
                {
                    Reason = "Test reason"
                }
            };

            await Assert.ThrowsAsync<InvalidApprovalStatusException>(() => handler.Handle(command));
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
            var role = context.Roles.FirstOrDefault(r => r.RoleName == roleName);
            if (role == null)
            {
                role = new Role
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = roleName
                };
                context.Roles.Add(role);
            }

            context.Users.Add(new User
            {
                UserId = userId,
                Email = $"{roleName.ToLower()}@example.com",
                FirstName = "Test",
                LastName = roleName,
                RoleId = role.RoleId,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        private static async Task<(DiscountApproval, Guid)> SeedPendingApproval(AppDbContext context)
        {
            var requestedByUserId = Guid.NewGuid();
            SeedUser(context, requestedByUserId, "SalesRep");

            var approverUserId = Guid.NewGuid();
            SeedUser(context, approverUserId, "Manager");

            var client = new Client
            {
                ClientId = Guid.NewGuid(),
                CompanyName = "Test Client",
                Email = "client@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Clients.Add(client);

            var quotation = new Quotation
            {
                QuotationId = Guid.NewGuid(),
                QuotationNumber = "QT-001",
                ClientId = client.ClientId,
                Client = client,
                CreatedByUserId = requestedByUserId,
                CreatedByUser = context.Users.First(u => u.UserId == requestedByUserId),
                Status = QuotationStatus.Draft,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                DiscountPercentage = 15.0m,
                SubTotal = 1000.0m,
                TaxAmount = 100.0m,
                TotalAmount = 1100.0m,
                IsPendingApproval = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Quotations.Add(quotation);

            var approval = new DiscountApproval
            {
                ApprovalId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                Quotation = quotation,
                Status = ApprovalStatus.Pending,
                ApprovalLevel = ApprovalLevel.Manager,
                CurrentDiscountPercentage = 15.0m,
                RequestedByUserId = requestedByUserId,
                RequestedByUser = context.Users.First(u => u.UserId == requestedByUserId),
                Reason = "Test reason",
                RequestDate = DateTimeOffset.UtcNow
            };
            context.DiscountApprovals.Add(approval);

            quotation.PendingApprovalId = approval.ApprovalId;
            quotation.PendingApproval = approval;

            await context.SaveChangesAsync();
            return (approval, approverUserId);
        }

        private static async Task<(DiscountApproval, Guid)> SeedRejectedApproval(AppDbContext context)
        {
            var (approval, approverUserId) = await SeedPendingApproval(context);
            approval.Status = ApprovalStatus.Rejected;
            approval.ApproverUserId = approverUserId;
            approval.RejectionDate = DateTimeOffset.UtcNow;
            approval.Quotation.IsPendingApproval = false;
            approval.Quotation.PendingApprovalId = null;
            await context.SaveChangesAsync();
            return (approval, approverUserId);
        }
    }
}

