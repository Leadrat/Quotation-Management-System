using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Products.Commands;
using CRM.Application.Products.Commands.Handlers;
using CRM.Application.Products.Queries;
using CRM.Application.Products.Queries.Handlers;
using CRM.Application.Products.Requests;
using CRM.Application.Products.Validators;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/product-categories")]
    [Authorize]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogger _audit;
        private readonly IMapper _mapper;

        public ProductCategoriesController(AppDbContext db, IAuditLogger audit, IMapper mapper)
        {
            _db = db;
            _audit = audit;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] Guid? parentCategoryId, [FromQuery] bool? isActive)
        {
            try
            {
                var query = new GetProductCategoriesQuery
                {
                    ParentCategoryId = parentCategoryId,
                    IsActive = isActive
                };

                var handler = new GetProductCategoriesQueryHandler(_db, _mapper);
                var result = await handler.Handle(query);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("product_categories_list_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving categories" });
            }
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategoryById(Guid categoryId)
        {
            try
            {
                var query = new GetProductCategoriesQuery { ParentCategoryId = null, IsActive = null };
                var handler = new GetProductCategoriesQueryHandler(_db, _mapper);
                var result = await handler.Handle(query);
                var category = result.FirstOrDefault(c => c.CategoryId == categoryId);

                if (category == null)
                {
                    return NotFound(new { success = false, error = "Category not found" });
                }

                return Ok(new { success = true, data = category });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("product_category_get_error", new { categoryId, error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving category" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateProductCategoryRequest body)
        {
            try
            {
                var validator = new CreateProductCategoryRequestValidator();
                var validationResult = validator.Validate(body);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, error = "Validation failed", errors = validationResult.ToDictionary() });
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub")
                    ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirstValue("userId");

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || userId == Guid.Empty)
                {
                    return Unauthorized(new { success = false, error = "Invalid user token - user ID not found" });
                }

                await _audit.LogAsync("product_category_create_attempt", new { userId, categoryCode = body.CategoryCode });

                var cmd = new CreateProductCategoryCommand
                {
                    CategoryName = body.CategoryName,
                    CategoryCode = body.CategoryCode,
                    Description = body.Description,
                    ParentCategoryId = body.ParentCategoryId,
                    IsActive = body.IsActive,
                    CreatedByUserId = userId
                };

                var handler = new CreateProductCategoryCommandHandler(_db, _mapper);
                var created = await handler.Handle(cmd);

                await _audit.LogAsync("product_category_create_success", new { userId, created.CategoryId });
                return StatusCode(201, new { success = true, message = "Category created successfully", data = created });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("product_category_create_validation_error", new { error = ex.Message });
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("product_category_create_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while creating category" });
            }
        }

        [HttpPut("{categoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(Guid categoryId, [FromBody] UpdateProductCategoryRequest body)
        {
            try
            {
                var validator = new UpdateProductCategoryRequestValidator();
                var validationResult = validator.Validate(body);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, error = "Validation failed", errors = validationResult.ToDictionary() });
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub")
                    ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirstValue("userId");

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || userId == Guid.Empty)
                {
                    return Unauthorized(new { success = false, error = "Invalid user token - user ID not found" });
                }

                await _audit.LogAsync("product_category_update_attempt", new { userId, categoryId });

                var cmd = new UpdateProductCategoryCommand
                {
                    CategoryId = categoryId,
                    CategoryName = body.CategoryName,
                    CategoryCode = body.CategoryCode,
                    Description = body.Description,
                    ParentCategoryId = body.ParentCategoryId,
                    IsActive = body.IsActive,
                    UpdatedByUserId = userId
                };

                var handler = new UpdateProductCategoryCommandHandler(_db, _mapper);
                var updated = await handler.Handle(cmd);

                await _audit.LogAsync("product_category_update_success", new { userId, categoryId });
                return Ok(new { success = true, message = "Category updated successfully", data = updated });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("product_category_update_validation_error", new { categoryId, error = ex.Message });
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("product_category_update_error", new { categoryId, error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while updating category" });
            }
        }
    }
}

