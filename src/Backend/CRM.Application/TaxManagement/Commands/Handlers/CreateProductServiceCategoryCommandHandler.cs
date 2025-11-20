using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Commands;
using CRM.Application.TaxManagement.Dtos;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Commands.Handlers
{
    public class CreateProductServiceCategoryCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public CreateProductServiceCategoryCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductServiceCategoryDto> Handle(CreateProductServiceCategoryCommand cmd)
        {
            // Validate category name uniqueness
            var nameLower = cmd.CategoryName.Trim().ToLowerInvariant();
            var existsByName = await _db.ProductServiceCategories.AnyAsync(c =>
                c.CategoryName.ToLower() == nameLower && c.DeletedAt == null);
            if (existsByName)
            {
                throw new InvalidOperationException($"Category with name '{cmd.CategoryName}' already exists");
            }

            // Validate category code uniqueness if provided
            if (!string.IsNullOrWhiteSpace(cmd.CategoryCode))
            {
                var codeUpper = cmd.CategoryCode.Trim().ToUpperInvariant();
                var existsByCode = await _db.ProductServiceCategories.AnyAsync(c =>
                    c.CategoryCode != null && c.CategoryCode.ToUpper() == codeUpper && c.DeletedAt == null);
                if (existsByCode)
                {
                    throw new InvalidOperationException($"Category with code '{cmd.CategoryCode}' already exists");
                }
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new ProductServiceCategory
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = cmd.CategoryName.Trim(),
                CategoryCode = string.IsNullOrWhiteSpace(cmd.CategoryCode) ? null : cmd.CategoryCode.Trim().ToUpperInvariant(),
                Description = string.IsNullOrWhiteSpace(cmd.Description) ? null : cmd.Description.Trim(),
                IsActive = cmd.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.ProductServiceCategories.Add(entity);
            await _db.SaveChangesAsync();

            return _mapper.Map<ProductServiceCategoryDto>(entity);
        }
    }
}

