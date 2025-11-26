using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/v1/tenants")]
    [Authorize(Roles = "SuperAdmin")]
    public class TenantsController : ControllerBase
    {
        private readonly IAppDbContext _db;

        public TenantsController(IAppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// List all tenants (SuperAdmin only).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<TenantDto>), 200)]
        public async Task<IActionResult> List()
        {
            var tenants = await _db.Tenants
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TenantDto
                {
                    Id = t.Id,
                    TenantId = t.TenantId,
                    Name = t.Name,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            return Ok(new { success = true, data = tenants });
        }

        /// <summary>
        /// Get a specific tenant by ID (SuperAdmin only).
        /// </summary>
        [HttpGet("{tenantId}")]
        [ProducesResponseType(typeof(TenantDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById([FromRoute] Guid tenantId)
        {
            var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
                return NotFound(new { error = "Tenant not found" });

            var dto = new TenantDto
            {
                Id = tenant.Id,
                TenantId = tenant.TenantId,
                Name = tenant.Name,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt,
                UpdatedAt = tenant.UpdatedAt
            };

            return Ok(new { success = true, data = dto });
        }

        /// <summary>
        /// Create a new tenant (SuperAdmin only).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TenantDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TenantId) || string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { error = "TenantId and Name are required" });

            // Check if TenantId already exists
            var existing = await _db.Tenants.FirstOrDefaultAsync(t => t.TenantId == request.TenantId);
            if (existing != null)
                return BadRequest(new { error = $"TenantId '{request.TenantId}' already exists" });

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                Name = request.Name,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _db.Tenants.AddAsync(tenant);
            await _db.SaveChangesAsync();

            var dto = new TenantDto
            {
                Id = tenant.Id,
                TenantId = tenant.TenantId,
                Name = tenant.Name,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt,
                UpdatedAt = tenant.UpdatedAt
            };

            return Created($"/api/v1/tenants/{tenant.Id}", new { success = true, data = dto });
        }

        /// <summary>
        /// Update a tenant (SuperAdmin only).
        /// </summary>
        [HttpPut("{tenantId}")]
        [ProducesResponseType(typeof(TenantDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Update([FromRoute] Guid tenantId, [FromBody] UpdateTenantRequest request)
        {
            var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
                return NotFound(new { error = "Tenant not found" });

            if (request == null)
                return BadRequest(new { error = "Request body is required" });

            // If TenantId is being changed, check for duplicates
            if (!string.IsNullOrWhiteSpace(request.TenantId) && request.TenantId != tenant.TenantId)
            {
                var duplicate = await _db.Tenants.FirstOrDefaultAsync(t => t.TenantId == request.TenantId);
                if (duplicate != null)
                    return BadRequest(new { error = $"TenantId '{request.TenantId}' already exists" });

                tenant.TenantId = request.TenantId;
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
                tenant.Name = request.Name;

            if (request.IsActive.HasValue)
                tenant.IsActive = request.IsActive.Value;

            tenant.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            var dto = new TenantDto
            {
                Id = tenant.Id,
                TenantId = tenant.TenantId,
                Name = tenant.Name,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt,
                UpdatedAt = tenant.UpdatedAt
            };

            return Ok(new { success = true, data = dto });
        }

        /// <summary>
        /// Deactivate a tenant (SuperAdmin only).
        /// </summary>
        [HttpPost("{tenantId}/deactivate")]
        [ProducesResponseType(typeof(TenantDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Deactivate([FromRoute] Guid tenantId)
        {
            var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null)
                return NotFound(new { error = "Tenant not found" });

            tenant.IsActive = false;
            tenant.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            var dto = new TenantDto
            {
                Id = tenant.Id,
                TenantId = tenant.TenantId,
                Name = tenant.Name,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt,
                UpdatedAt = tenant.UpdatedAt
            };

            return Ok(new { success = true, data = dto });
        }
    }

    public class TenantDto
    {
        public Guid Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public class CreateTenantRequest
    {
        public string TenantId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateTenantRequest
    {
        public string? TenantId { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
    }
}
