using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Products.DTOs;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Products.Commands.Handlers
{
    public class UpdateProductCategoryCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public UpdateProductCategoryCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductCategoryDto> Handle(UpdateProductCategoryCommand cmd)
        {
            var category = await _db.ProductCategories
                .FirstOrDefaultAsync(c => c.CategoryId == cmd.CategoryId);

            if (category == null)
            {
                throw new InvalidOperationException($"Product category with ID {cmd.CategoryId} not found.");
            }

            // Validate user exists
            var userExists = await _db.Users.AnyAsync(u => u.UserId == cmd.UpdatedByUserId);
            if (!userExists)
            {
                throw new InvalidOperationException($"User with ID {cmd.UpdatedByUserId} not found.");
            }

            // Check for duplicate category code, excluding the current category
            var existingCategoryWithSameCode = await _db.ProductCategories
                .FirstOrDefaultAsync(c => c.CategoryId != cmd.CategoryId && c.CategoryCode.ToLower() == cmd.CategoryCode.ToLower());
            if (existingCategoryWithSameCode != null)
            {
                throw new InvalidOperationException($"Category code '{cmd.CategoryCode}' is already in use.");
            }

            // Validate parent category if provided
            if (cmd.ParentCategoryId.HasValue)
            {
                var parentExists = await _db.ProductCategories.AnyAsync(c => c.CategoryId == cmd.ParentCategoryId.Value);
                if (!parentExists)
                {
                    throw new InvalidOperationException($"Parent category with ID {cmd.ParentCategoryId.Value} not found.");
                }

                // Prevent circular reference
                if (cmd.ParentCategoryId.Value == cmd.CategoryId)
                {
                    throw new InvalidOperationException("A category cannot be its own parent.");
                }
            }

            // Update category properties
            category.CategoryName = cmd.CategoryName;
            category.CategoryCode = cmd.CategoryCode;
            category.Description = cmd.Description;
            category.ParentCategoryId = cmd.ParentCategoryId;
            category.IsActive = cmd.IsActive;
            category.UpdatedByUserId = cmd.UpdatedByUserId;
            category.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            // Reload with navigation properties
            var updatedCategory = await _db.ProductCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);

            return _mapper.Map<ProductCategoryDto>(updatedCategory);
        }
    }
}

