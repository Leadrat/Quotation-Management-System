using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.TaxManagement.Commands;
using CRM.Application.TaxManagement.Commands.Handlers;
using CRM.Application.TaxManagement.Queries;
using CRM.Application.TaxManagement.Queries.Handlers;
using CRM.Application.TaxManagement.Requests;
using CRM.Application.TaxManagement.Validators;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using CRM.Api.Filters;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/tax/categories")]
    [AdminOnly]
    public class ProductServiceCategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogger _audit;
        private readonly IMapper _mapper;

        public ProductServiceCategoriesController(AppDbContext db, IAuditLogger audit, IMapper mapper)
        {
            _db = db;
            _audit = audit;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
        {
            var handler = new GetAllProductServiceCategoriesQueryHandler(_db, _mapper);
            var query = new GetAllProductServiceCategoriesQuery
            {
                IsActive = isActive
            };
            var result = await handler.Handle(query);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetById([FromRoute] Guid categoryId)
        {
            var handler = new GetProductServiceCategoryByIdQueryHandler(_db, _mapper);
            var query = new GetProductServiceCategoryByIdQuery
            {
                CategoryId = categoryId
            };
            var result = await handler.Handle(query);

            if (result == null)
            {
                return NotFound(new { success = false, message = "Category not found" });
            }

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductServiceCategoryRequest request)
        {
            var validator = new CreateProductServiceCategoryRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { success = false, errors = validationResult.Errors });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
            var handler = new CreateProductServiceCategoryCommandHandler(_db, _mapper);
            var command = new CreateProductServiceCategoryCommand
            {
                CategoryName = request.CategoryName,
                CategoryCode = request.CategoryCode,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedByUserId = userId
            };

            try
            {
                var result = await handler.Handle(command);
                await _audit.LogAsync("product_service_category_created", new { userId, result.CategoryId, result.CategoryName });

                return CreatedAtAction(nameof(GetById), new { categoryId = result.CategoryId }, new
                {
                    success = true,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> Update([FromRoute] Guid categoryId, [FromBody] UpdateProductServiceCategoryRequest request)
        {
            var validator = new UpdateProductServiceCategoryRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { success = false, errors = validationResult.Errors });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException());
            var handler = new UpdateProductServiceCategoryCommandHandler(_db, _mapper);
            var command = new UpdateProductServiceCategoryCommand
            {
                CategoryId = categoryId,
                CategoryName = request.CategoryName,
                CategoryCode = request.CategoryCode,
                Description = request.Description,
                IsActive = request.IsActive,
                UpdatedByUserId = userId
            };

            try
            {
                var result = await handler.Handle(command);
                await _audit.LogAsync("product_service_category_updated", new { userId, categoryId, result.CategoryName });

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}

