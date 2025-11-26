using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Commands;
using CRM.Application.TaxManagement.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Commands.Handlers
{
    public class UpdateProductServiceCategoryCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public UpdateProductServiceCategoryCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductServiceCategoryDto> Handle(UpdateProductServiceCategoryCommand cmd)
        {
            var entity = await _db.ProductServiceCategories
                .FirstOrDefaultAsync(c => c.CategoryId == cmd.CategoryId && c.DeletedAt == null);

            if (entity == null)
            {
                throw new InvalidOperationException($"Category with ID '{cmd.CategoryId}' not found");
            }

            // Validate category name uniqueness
            var nameLower = cmd.CategoryName.Trim().ToLowerInvariant();
            var existsByName = await _db.ProductServiceCategories.AnyAsync(c =>
                c.CategoryId != cmd.CategoryId &&
                c.CategoryName.ToLower() == nameLower &&
                c.DeletedAt == null);
            if (existsByName)
            {
                throw new InvalidOperationException($"Category with name '{cmd.CategoryName}' already exists");
            }

            // Validate category code uniqueness if provided
            if (!string.IsNullOrWhiteSpace(cmd.CategoryCode))
            {
                var codeUpper = cmd.CategoryCode.Trim().ToUpperInvariant();
                var existsByCode = await _db.ProductServiceCategories.AnyAsync(c =>
                    c.CategoryId != cmd.CategoryId &&
                    c.CategoryCode != null &&
                    c.CategoryCode.ToUpper() == codeUpper &&
                    c.DeletedAt == null);
                if (existsByCode)
                {
                    throw new InvalidOperationException($"Category with code '{cmd.CategoryCode}' already exists");
                }
            }

            entity.CategoryName = cmd.CategoryName.Trim();
            entity.CategoryCode = string.IsNullOrWhiteSpace(cmd.CategoryCode) ? null : cmd.CategoryCode.Trim().ToUpperInvariant();
            entity.Description = string.IsNullOrWhiteSpace(cmd.Description) ? null : cmd.Description.Trim();
            entity.IsActive = cmd.IsActive;
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return _mapper.Map<ProductServiceCategoryDto>(entity);
        }
    }
}

