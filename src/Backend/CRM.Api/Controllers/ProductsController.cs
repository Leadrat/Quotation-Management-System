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
using CRM.Application.Common.Results;
using CRM.Infrastructure.Logging;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/products")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAuditLogger _audit;
        private readonly IMapper _mapper;
        private readonly CRM.Application.Products.Services.IProductPricingService _pricingService;
        private readonly CRM.Application.Localization.Services.ICurrencyService _currencyService;
        private readonly ILoggerFactory _loggerFactory;

        public ProductsController(
            AppDbContext db, 
            IAuditLogger audit, 
            IMapper mapper, 
            CRM.Application.Products.Services.IProductPricingService pricingService,
            CRM.Application.Localization.Services.ICurrencyService currencyService,
            ILoggerFactory loggerFactory)
        {
            _db = db;
            _audit = audit;
            _mapper = mapper;
            _pricingService = pricingService;
            _currencyService = currencyService;
            _loggerFactory = loggerFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? productType = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? search = null,
            [FromQuery] string? currency = null)
        {
            try
            {
                var query = new GetProductsQuery
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    ProductType = productType.HasValue ? (Domain.Enums.ProductType)productType.Value : null,
                    CategoryId = categoryId,
                    IsActive = isActive,
                    Search = search,
                    Currency = currency
                };

                var handler = new GetProductsQueryHandler(_db, _mapper);
                var result = await handler.Handle(query);

                return Ok(new { success = true, data = result.Data, pageNumber = result.PageNumber, pageSize = result.PageSize, totalCount = result.TotalCount });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("products_list_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving products" });
            }
        }

        [HttpGet("catalog")]
        public async Task<IActionResult> GetCatalog(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? productType = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] string? search = null,
            [FromQuery] string? currency = null)
        {
            try
            {
                // Only return active products for catalog
                var query = new GetProductsQuery
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    ProductType = productType.HasValue ? (Domain.Enums.ProductType)productType.Value : null,
                    CategoryId = categoryId,
                    IsActive = true, // Catalog only shows active products
                    Search = search,
                    Currency = currency
                };

                var handler = new GetProductsQueryHandler(_db, _mapper);
                var result = await handler.Handle(query);

                // Transform to catalog format
                var catalogItems = result.Data.Select(p => new
                {
                    productId = p.ProductId,
                    productName = p.ProductName,
                    productType = p.ProductType.ToString(),
                    description = p.Description,
                    categoryId = p.CategoryId,
                    categoryName = p.CategoryName,
                    basePricePerUserPerMonth = p.BasePricePerUserPerMonth,
                    currency = p.Currency,
                    pricingSummary = p.BasePricePerUserPerMonth.HasValue
                        ? $"{p.Currency} {p.BasePricePerUserPerMonth.Value:F2}/user/month"
                        : "Price on request"
                }).ToArray();

                return Ok(new { success = true, data = catalogItems, pageNumber = result.PageNumber, pageSize = result.PageSize, totalCount = result.TotalCount });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("products_catalog_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving product catalog" });
            }
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(Guid productId)
        {
            try
            {
                var query = new GetProductByIdQuery { ProductId = productId };
                var handler = new GetProductByIdQueryHandler(_db, _mapper);
                var result = await handler.Handle(query);

                if (result == null)
                {
                    return NotFound(new { success = false, error = "Product not found" });
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("product_get_error", new { productId, error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving product" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest body)
        {
            try
            {
                var validator = new CreateProductRequestValidator();
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

                await _audit.LogAsync("product_create_attempt", new { userId, productName = body.ProductName });

                var cmd = new CreateProductCommand
                {
                    ProductName = body.ProductName,
                    ProductType = body.ProductType,
                    Description = body.Description,
                    CategoryId = body.CategoryId,
                    BasePricePerUserPerMonth = body.BasePricePerUserPerMonth,
                    BillingCycleMultipliers = body.BillingCycleMultipliers,
                    AddOnPricing = body.AddOnPricing,
                    CustomDevelopmentPricing = body.CustomDevelopmentPricing,
                    Currency = body.Currency,
                    IsActive = body.IsActive,
                    CreatedByUserId = userId
                };

                var handler = new CreateProductCommandHandler(_db, _mapper, _pricingService);
                var created = await handler.Handle(cmd);

                await _audit.LogAsync("product_create_success", new { userId, created.ProductId });
                return StatusCode(201, new { success = true, message = "Product created successfully", data = created });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("product_create_validation_error", new { error = ex.Message });
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("product_create_error", new { error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while creating product" });
            }
        }

        [HttpPut("{productId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(Guid productId, [FromBody] CreateProductRequest body)
        {
            try
            {
                var validator = new CreateProductRequestValidator();
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

                await _audit.LogAsync("product_update_attempt", new { userId, productId });

                var cmd = new UpdateProductCommand
                {
                    ProductId = productId,
                    ProductName = body.ProductName,
                    ProductType = body.ProductType,
                    Description = body.Description,
                    CategoryId = body.CategoryId,
                    BasePricePerUserPerMonth = body.BasePricePerUserPerMonth,
                    BillingCycleMultipliers = body.BillingCycleMultipliers,
                    AddOnPricing = body.AddOnPricing,
                    CustomDevelopmentPricing = body.CustomDevelopmentPricing,
                    Currency = body.Currency,
                    IsActive = body.IsActive,
                    UpdatedByUserId = userId
                };

                var handler = new UpdateProductCommandHandler(_db, _mapper, _pricingService);
                var updated = await handler.Handle(cmd);

                await _audit.LogAsync("product_update_success", new { userId, productId });
                return Ok(new { success = true, message = "Product updated successfully", data = updated });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("product_update_validation_error", new { productId, error = ex.Message });
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("product_update_error", new { productId, error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while updating product" });
            }
        }

        [HttpDelete("{productId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(Guid productId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub")
                    ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirstValue("userId");

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || userId == Guid.Empty)
                {
                    return Unauthorized(new { success = false, error = "Invalid user token - user ID not found" });
                }

                await _audit.LogAsync("product_delete_attempt", new { userId, productId });

                var cmd = new DeleteProductCommand
                {
                    ProductId = productId,
                    DeletedByUserId = userId
                };

                var handler = new DeleteProductCommandHandler(_db);
                await handler.Handle(cmd);

                await _audit.LogAsync("product_delete_success", new { userId, productId });
                return Ok(new { success = true, message = "Product deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("product_delete_validation_error", new { productId, error = ex.Message });
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("product_delete_error", new { productId, error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while deleting product" });
            }
        }

        [HttpPost("calculate-price")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> CalculatePrice([FromBody] CalculateProductPriceRequest body)
        {
            try
            {
                var product = await _db.Products
                    .FirstOrDefaultAsync(p => p.ProductId == body.ProductId && p.IsActive);

                if (product == null)
                {
                    return NotFound(new { success = false, error = "Product not found or is inactive" });
                }

                Domain.Enums.BillingCycle? billingCycle = body.BillingCycle.HasValue ? (Domain.Enums.BillingCycle)body.BillingCycle.Value : null;
                var calculatedPrice = _pricingService.CalculatePrice(product, body.Quantity, billingCycle, body.Hours);
                var unitRate = body.Quantity > 0 ? calculatedPrice / body.Quantity : calculatedPrice;

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        productId = product.ProductId,
                        productName = product.ProductName,
                        unitRate = unitRate,
                        quantity = body.Quantity,
                        billingCycle = billingCycle?.ToString(),
                        hours = body.Hours,
                        subtotal = calculatedPrice,
                        currency = product.Currency
                    }
                });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("product_calculate_price_error", new { productId = body.ProductId, error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while calculating price" });
            }
        }

        [HttpGet("{productId}/usage")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProductUsageStats(Guid productId)
        {
            try
            {
                var query = new GetProductUsageStatsQuery { ProductId = productId };
                var handler = new GetProductUsageStatsQueryHandler(_db, _mapper);
                var result = await handler.Handle(query);

                if (result == null)
                {
                    return NotFound(new { success = false, error = "Product not found" });
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("product_usage_stats_error", new { productId, error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving product usage stats" });
            }
        }

        [HttpPost("quotations/{quotationId}/add-product")]
        [Authorize(Roles = "SalesRep,Admin")]
        public async Task<IActionResult> AddProductToQuotation(Guid quotationId, [FromBody] AddProductToQuotationRequest body)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub")
                    ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirstValue("userId");

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || userId == Guid.Empty)
                {
                    return Unauthorized(new { success = false, error = "Invalid user token - user ID not found" });
                }

                await _audit.LogAsync("product_add_to_quotation_attempt", new { userId, quotationId, productId = body.ProductId });

                var cmd = new AddProductToQuotationCommand
                {
                    QuotationId = quotationId,
                    ProductId = body.ProductId,
                    Quantity = body.Quantity,
                    BillingCycle = body.BillingCycle.HasValue ? (Domain.Enums.BillingCycle)body.BillingCycle.Value : null,
                    Hours = body.Hours,
                    TaxCategoryId = body.TaxCategoryId,
                    AddedByUserId = userId
                };

                var handler = new AddProductToQuotationCommandHandler(_db, _mapper, _pricingService, _currencyService, _loggerFactory.CreateLogger<CRM.Application.Products.Commands.Handlers.AddProductToQuotationCommandHandler>());
                var lineItem = await handler.Handle(cmd);

                await _audit.LogAsync("product_add_to_quotation_success", new { userId, quotationId, lineItem.LineItemId });
                return Ok(new { success = true, message = "Product added to quotation successfully", data = lineItem });
            }
            catch (InvalidOperationException ex)
            {
                await _audit.LogAsync("product_add_to_quotation_validation_error", new { quotationId, error = ex.Message });
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync("product_add_to_quotation_error", new { quotationId, error = ex.Message });
                return StatusCode(500, new { success = false, error = "An error occurred while adding product to quotation" });
            }
        }
    }

    public class AddProductToQuotationRequest
    {
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int? BillingCycle { get; set; }
        public decimal? Hours { get; set; }
        public Guid? TaxCategoryId { get; set; }
    }

    public class CalculateProductPriceRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public int? BillingCycle { get; set; }
        public decimal? Hours { get; set; }
    }
}
