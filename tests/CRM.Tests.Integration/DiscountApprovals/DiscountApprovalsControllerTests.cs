using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Api.Controllers;
using CRM.Application.Common.Results;
using CRM.Application.DiscountApprovals.Commands;
using CRM.Application.DiscountApprovals.Commands.Handlers;
using CRM.Application.DiscountApprovals.Dtos;
using CRM.Application.DiscountApprovals.Queries;
using CRM.Application.DiscountApprovals.Queries.Handlers;
using CRM.Application.DiscountApprovals.Validators;
using CRM.Application.Mapping;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.Integration.DiscountApprovals
{
    public class DiscountApprovalsControllerTests
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
            var cfg = new MapperConfiguration(c =>
            {
                c.AddProfile(new DiscountApprovalProfile());
                c.AddProfile(new QuotationProfile());
                c.AddProfile(new ClientProfile());
                c.AddProfile(new UserProfile());
            });
            return cfg.CreateMapper();
        }

        private static ClaimsPrincipal SalesRep(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("role", "SalesRep")
            }, "TestAuth"));
        }

        private static ClaimsPrincipal Manager(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("role", "Manager")
            }, "TestAuth"));
        }

        private static ClaimsPrincipal Admin(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("role", "Admin")
            }, "TestAuth"));
        }

        private static DiscountApprovalsController NewController(
            AppDbContext db,
            IMapper mapper,
            ClaimsPrincipal user)
        {
            var requestHandler = new RequestDiscountApprovalCommandHandler(db, mapper, NullLogger<RequestDiscountApprovalCommandHandler>.Instance);
            var approveHandler = new ApproveDiscountApprovalCommandHandler(db, mapper, NullLogger<ApproveDiscountApprovalCommandHandler>.Instance);
            var rejectHandler = new RejectDiscountApprovalCommandHandler(db, mapper, NullLogger<RejectDiscountApprovalCommandHandler>.Instance);
            var escalateHandler = new EscalateDiscountApprovalCommandHandler(db, mapper, NullLogger<EscalateDiscountApprovalCommandHandler>.Instance);
            var resubmitHandler = new ResubmitDiscountApprovalCommandHandler(db, mapper, NullLogger<ResubmitDiscountApprovalCommandHandler>.Instance);
            var bulkApproveHandler = new BulkApproveDiscountApprovalsCommandHandler(db, mapper, NullLogger<BulkApproveDiscountApprovalsCommandHandler>.Instance);
            var getPendingHandler = new GetPendingApprovalsQueryHandler(db, mapper);
            var getByIdHandler = new GetApprovalByIdQueryHandler(db, mapper);
            var getTimelineHandler = new GetApprovalTimelineQueryHandler(db, mapper);
            var getQuotationApprovalsHandler = new GetQuotationApprovalsQueryHandler(db, mapper);
            var getMetricsHandler = new GetApprovalMetricsQueryHandler(db, mapper);

            var requestValidator = new RequestDiscountApprovalCommandValidator();
            var approveValidator = new ApproveDiscountApprovalCommandValidator();
            var rejectValidator = new RejectDiscountApprovalCommandValidator();
            var escalateValidator = new EscalateDiscountApprovalCommandValidator();
            var resubmitValidator = new ResubmitDiscountApprovalCommandValidator();
            var bulkApproveValidator = new BulkApproveDiscountApprovalsCommandValidator();

            var controller = new DiscountApprovalsController(
                requestHandler, approveHandler, rejectHandler, escalateHandler, resubmitHandler, bulkApproveHandler,
                getPendingHandler, getByIdHandler, getTimelineHandler, getQuotationApprovalsHandler, getMetricsHandler,
                requestValidator, approveValidator, rejectValidator, escalateValidator, resubmitValidator, bulkApproveValidator);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return controller;
        }

        private static (Guid quotationId, Guid salesRepId, Guid managerId) SeedData(AppDbContext db)
        {
            var salesRepId = Guid.NewGuid();
            var managerId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            var salesRepRole = new Role { RoleId = Guid.NewGuid(), RoleName = "SalesRep", IsActive = true, CreatedAt = now, UpdatedAt = now };
            var managerRole = new Role { RoleId = Guid.NewGuid(), RoleName = "Manager", IsActive = true, CreatedAt = now, UpdatedAt = now };
            var adminRole = new Role { RoleId = Guid.NewGuid(), RoleName = "Admin", IsActive = true, CreatedAt = now, UpdatedAt = now };

            db.Roles.AddRange(salesRepRole, managerRole, adminRole);

            db.Users.Add(new User
            {
                UserId = salesRepId,
                Email = "salesrep@example.com",
                FirstName = "Sales",
                LastName = "Rep",
                RoleId = salesRepRole.RoleId,
                Role = salesRepRole,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            db.Users.Add(new User
            {
                UserId = managerId,
                Email = "manager@example.com",
                FirstName = "Manager",
                LastName = "User",
                RoleId = managerRole.RoleId,
                Role = managerRole,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            var client = new Client
            {
                ClientId = Guid.NewGuid(),
                CompanyName = "Test Client",
                Email = "client@example.com",
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Clients.Add(client);

            var quotation = new Quotation
            {
                QuotationId = Guid.NewGuid(),
                QuotationNumber = "QT-001",
                ClientId = client.ClientId,
                Client = client,
                CreatedByUserId = salesRepId,
                CreatedByUser = db.Users.First(u => u.UserId == salesRepId),
                Status = QuotationStatus.Draft,
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                DiscountPercentage = 15.0m,
                SubTotal = 1000.0m,
                TaxPercentage = 10.0m,
                TaxAmount = 100.0m,
                TotalAmount = 1100.0m,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Quotations.Add(quotation);

            db.SaveChanges();
            return (quotation.QuotationId, salesRepId, managerId);
        }

        [Fact]
        public async Task RequestApproval_ValidRequest_ReturnsCreated()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, salesRepId, _) = SeedData(db);
            var user = SalesRep(salesRepId);
            var controller = NewController(db, mapper, user);

            var request = new CreateDiscountApprovalRequest
            {
                QuotationId = quotationId,
                RequestedDiscountPercentage = 15.0m,
                Reason = "Competitive pricing required for this strategic client",
                Comments = "Client requested discount"
            };

            var result = await controller.RequestApproval(request);

            var createdResult = Assert.IsType<CreatedResult>(result);
            var response = createdResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.True((bool)response.success);
            var data = response.data as DiscountApprovalDto;
            Assert.NotNull(data);
            Assert.Equal(ApprovalStatus.Pending, data.Status);
            Assert.Equal(ApprovalLevel.Manager, data.ApprovalLevel);
        }

        [Fact]
        public async Task Approve_ValidRequest_ReturnsOk()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, salesRepId, managerId) = SeedData(db);

            // Create approval request
            var salesRepUser = SalesRep(salesRepId);
            var requestController = NewController(db, mapper, salesRepUser);
            var request = new CreateDiscountApprovalRequest
            {
                QuotationId = quotationId,
                RequestedDiscountPercentage = 15.0m,
                Reason = "Test reason for approval"
            };
            var requestResult = await requestController.RequestApproval(request);
            var requestResponse = Assert.IsType<CreatedResult>(requestResult);
            var requestData = Assert.IsType<ApiResponse<DiscountApprovalDto>>(requestResponse.Value);
            var approvalId = requestData.Data.ApprovalId;

            // Approve
            var managerUser = Manager(managerId);
            var approveController = NewController(db, mapper, managerUser);
            var approveRequest = new ApproveDiscountApprovalRequest
            {
                Reason = "Approved based on client relationship"
            };

            var result = await approveController.Approve(approvalId, approveRequest);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.True((bool)response.success);
            var data = response.data as DiscountApprovalDto;
            Assert.Equal(ApprovalStatus.Approved, data.Status);
            Assert.NotNull(data.ApprovalDate);
        }

        [Fact]
        public async Task Reject_ValidRequest_ReturnsOk()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, salesRepId, managerId) = SeedData(db);

            // Create approval request
            var salesRepUser = SalesRep(salesRepId);
            var requestController = NewController(db, mapper, salesRepUser);
            var request = new CreateDiscountApprovalRequest
            {
                QuotationId = quotationId,
                RequestedDiscountPercentage = 15.0m,
                Reason = "Test reason for approval"
            };
            var requestResult = await requestController.RequestApproval(request);
            var requestResponse = Assert.IsType<CreatedResult>(requestResult);
            var requestResponseValue = requestResponse.Value as dynamic;
            var approvalId = (requestResponseValue.data as DiscountApprovalDto).ApprovalId;

            // Reject
            var managerUser = Manager(managerId);
            var rejectController = NewController(db, mapper, managerUser);
            var rejectRequest = new RejectDiscountApprovalRequest
            {
                Reason = "Discount too high for this client segment"
            };

            var result = await rejectController.Reject(approvalId, rejectRequest);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.True((bool)response.success);
            var data = response.data as DiscountApprovalDto;
            Assert.Equal(ApprovalStatus.Rejected, data.Status);
            Assert.NotNull(data.RejectionDate);
        }

        [Fact]
        public async Task GetPending_ReturnsPendingApprovals()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, salesRepId, managerId) = SeedData(db);

            // Create approval request
            var salesRepUser = SalesRep(salesRepId);
            var requestController = NewController(db, mapper, salesRepUser);
            var request = new CreateDiscountApprovalRequest
            {
                QuotationId = quotationId,
                RequestedDiscountPercentage = 15.0m,
                Reason = "Test reason for approval"
            };
            await requestController.RequestApproval(request);

            // Get pending
            var managerUser = Manager(managerId);
            var getController = NewController(db, mapper, managerUser);

            var result = await getController.GetPending(null, null, null, null, null, null, null, 1, 20);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.True((bool)response.success);
            var pagedData = response.data as PagedResult<DiscountApprovalDto>;
            Assert.NotNull(pagedData);
            Assert.True(pagedData.Data.Length > 0);
            Assert.All(pagedData.Data, a => Assert.Equal(ApprovalStatus.Pending, a.Status));
        }

        [Fact]
        public async Task GetById_ReturnsApproval()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (quotationId, salesRepId, _) = SeedData(db);

            // Create approval request
            var salesRepUser = SalesRep(salesRepId);
            var requestController = NewController(db, mapper, salesRepUser);
            var request = new CreateDiscountApprovalRequest
            {
                QuotationId = quotationId,
                RequestedDiscountPercentage = 15.0m,
                Reason = "Test reason for approval"
            };
            var requestResult = await requestController.RequestApproval(request);
            var requestResponse = Assert.IsType<CreatedResult>(requestResult);
            var requestResponseValue = requestResponse.Value as dynamic;
            var approvalId = (requestResponseValue.data as DiscountApprovalDto).ApprovalId;

            // Get by ID
            var getController = NewController(db, mapper, salesRepUser);

            var result = await getController.GetById(approvalId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.True((bool)response.success);
            var data = response.data as DiscountApprovalDto;
            Assert.NotNull(data);
            Assert.Equal(approvalId, data.ApprovalId);
        }
    }
}

