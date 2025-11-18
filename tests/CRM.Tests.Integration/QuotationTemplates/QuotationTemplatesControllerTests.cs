using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Api.Controllers;
using CRM.Application.Mapping;
using CRM.Application.QuotationTemplates.Commands.Handlers;
using CRM.Application.QuotationTemplates.Queries.Handlers;
using CRM.Application.QuotationTemplates.Validators;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.Integration.QuotationTemplates
{
    public class QuotationTemplatesControllerTests
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
                c.AddProfile(new QuotationTemplateProfile());
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

        private static ClaimsPrincipal Admin(Guid userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("role", "Admin")
            }, "TestAuth"));
        }

        private static QuotationTemplatesController NewController(AppDbContext db, IMapper mapper, ClaimsPrincipal user)
        {
            var createHandler = new CreateQuotationTemplateCommandHandler(db, mapper, NullLogger<CreateQuotationTemplateCommandHandler>.Instance);
            var updateHandler = new UpdateQuotationTemplateCommandHandler(db, mapper, NullLogger<UpdateQuotationTemplateCommandHandler>.Instance);
            var deleteHandler = new DeleteQuotationTemplateCommandHandler(db, NullLogger<DeleteQuotationTemplateCommandHandler>.Instance);
            var restoreHandler = new RestoreQuotationTemplateCommandHandler(db, NullLogger<RestoreQuotationTemplateCommandHandler>.Instance);
            var approveHandler = new ApproveQuotationTemplateCommandHandler(db, mapper, NullLogger<ApproveQuotationTemplateCommandHandler>.Instance);
            var applyHandler = new ApplyTemplateToQuotationCommandHandler(db, mapper, NullLogger<ApplyTemplateToQuotationCommandHandler>.Instance);
            var getByIdHandler = new GetTemplateByIdQueryHandler(db, mapper, NullLogger<GetTemplateByIdQueryHandler>.Instance);
            var getAllHandler = new GetAllTemplatesQueryHandler(db, mapper, NullLogger<GetAllTemplatesQueryHandler>.Instance);
            var getVersionsHandler = new GetTemplateVersionsQueryHandler(db, mapper, NullLogger<GetTemplateVersionsQueryHandler>.Instance);
            var getPublicHandler = new GetPublicTemplatesQueryHandler(db, mapper, NullLogger<GetPublicTemplatesQueryHandler>.Instance);
            var getUsageStatsHandler = new GetTemplateUsageStatsQueryHandler(db, mapper, NullLogger<GetTemplateUsageStatsQueryHandler>.Instance);

            var createValidator = new CreateQuotationTemplateRequestValidator();
            var updateValidator = new UpdateQuotationTemplateRequestValidator();
            var approveValidator = new ApproveQuotationTemplateCommandValidator();
            var applyValidator = new ApplyTemplateToQuotationCommandValidator();

            var ctrl = new QuotationTemplatesController(
                createHandler, updateHandler, deleteHandler, restoreHandler, approveHandler, applyHandler,
                getByIdHandler, getAllHandler, getVersionsHandler, getPublicHandler, getUsageStatsHandler,
                createValidator, updateValidator, approveValidator, applyValidator);
            ctrl.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return ctrl;
        }

        private static (Guid userId, Guid templateId) SeedData(AppDbContext db)
        {
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var templateId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            db.Roles.Add(new Role
            {
                RoleId = roleId,
                RoleName = "SalesRep",
                IsActive = true
            });

            db.Users.Add(new User
            {
                UserId = userId,
                Email = "sales@test.com",
                FirstName = "Sales",
                LastName = "Rep",
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = "hash"
            });

            db.QuotationTemplates.Add(new QuotationTemplate
            {
                TemplateId = templateId,
                Name = "Test Template",
                OwnerUserId = userId,
                OwnerRole = "SalesRep",
                Visibility = TemplateVisibility.Private,
                Version = 1,
                CreatedAt = now,
                UpdatedAt = now
            });

            db.SaveChanges();
            return (userId, templateId);
        }

        [Fact]
        public async Task Create_ReturnsCreatedTemplate()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (userId, _) = SeedData(db);
            var controller = NewController(db, mapper, SalesRep(userId));

            var request = new CRM.Application.QuotationTemplates.Dtos.CreateQuotationTemplateRequest
            {
                Name = "New Template",
                Description = "Test Description",
                Visibility = "Private",
                LineItems = new List<CRM.Application.QuotationTemplates.Dtos.CreateTemplateLineItemRequest>
                {
                    new CRM.Application.QuotationTemplates.Dtos.CreateTemplateLineItemRequest
                    {
                        ItemName = "Item 1",
                        Quantity = 1,
                        UnitRate = 100
                    }
                }
            };

            var result = await controller.Create(request);

            var createdResult = Assert.IsType<CreatedResult>(result);
            var response = createdResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.True((bool)response.success);
            Assert.Equal("New Template", (response.data as CRM.Application.QuotationTemplates.Dtos.QuotationTemplateDto).Name);
        }

        [Fact]
        public async Task GetById_ReturnsTemplate()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (userId, templateId) = SeedData(db);
            var controller = NewController(db, mapper, SalesRep(userId));

            var result = await controller.GetById(templateId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.True((bool)response.success);
            Assert.Equal(templateId, (response.data as CRM.Application.QuotationTemplates.Dtos.QuotationTemplateDto).TemplateId);
        }

        [Fact]
        public async Task List_ReturnsTemplates()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (userId, _) = SeedData(db);
            var controller = NewController(db, mapper, SalesRep(userId));

            var result = await controller.List();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.True((bool)response.success);
        }

        [Fact]
        public async Task Approve_AdminOnly_ApprovesTemplate()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (userId, templateId) = SeedData(db);
            var adminId = Guid.NewGuid();
            var adminRoleId = Guid.NewGuid();

            db.Roles.Add(new Role { RoleId = adminRoleId, RoleName = "Admin", IsActive = true });
            db.Users.Add(new User
            {
                UserId = adminId,
                Email = "admin@test.com",
                FirstName = "Admin",
                LastName = "User",
                RoleId = adminRoleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = "hash"
            });
            db.SaveChanges();

            var controller = NewController(db, mapper, Admin(adminId));

            var result = await controller.Approve(templateId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.True((bool)response.success);
            Assert.True((response.data as CRM.Application.QuotationTemplates.Dtos.QuotationTemplateDto).IsApproved);
        }

        [Fact]
        public async Task Delete_SoftDeletesTemplate()
        {
            using var db = NewDb();
            var mapper = NewMapper();
            var (userId, templateId) = SeedData(db);
            var controller = NewController(db, mapper, SalesRep(userId));

            var result = await controller.Delete(templateId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.True((bool)response.success);

            var deleted = await db.QuotationTemplates.FirstAsync(t => t.TemplateId == templateId);
            Assert.NotNull(deleted.DeletedAt);
        }
    }
}

