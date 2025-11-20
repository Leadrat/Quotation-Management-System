using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Products.DTOs;
using CRM.Domain.Entities;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Products.Commands.Handlers
{
    public class CreateProductCategoryCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public CreateProductCategoryCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ProductCategoryDto> Handle(CreateProductCategoryCommand cmd)
        {
            // Validate user exists
            var userExists = await _db.Users.AnyAsync(u => u.UserId == cmd.CreatedByUserId && u.DeletedAt == null);
            if (!userExists)
            {
                throw new InvalidOperationException($"User with ID {cmd.CreatedByUserId} does not exist.");
            }

            // Check for duplicate category code
            var codeExists = await _db.ProductCategories.AnyAsync(c => c.CategoryCode == cmd.CategoryCode);
            if (codeExists)
            {
                throw new InvalidOperationException($"Category code '{cmd.CategoryCode}' already exists.");
            }

            // Validate parent category if provided
            if (cmd.ParentCategoryId.HasValue)
            {
                var parentExists = await _db.ProductCategories.AnyAsync(c => c.CategoryId == cmd.ParentCategoryId.Value && c.IsActive);
                if (!parentExists)
                {
                    throw new InvalidOperationException($"Parent category with ID {cmd.ParentCategoryId.Value} does not exist or is inactive.");
                }
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new ProductCategory
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = cmd.CategoryName,
                CategoryCode = cmd.CategoryCode,
                Description = cmd.Description,
                ParentCategoryId = cmd.ParentCategoryId,
                IsActive = cmd.IsActive,
                CreatedByUserId = cmd.CreatedByUserId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.ProductCategories.Add(entity);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new InvalidOperationException($"Database error while saving category: {innerException}", dbEx);
            }

            // Reload with navigation properties
            var entityWithNav = await _db.ProductCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryId == entity.CategoryId);

            if (entityWithNav == null)
            {
                throw new InvalidOperationException("Failed to retrieve created category");
            }

            return _mapper.Map<ProductCategoryDto>(entityWithNav);
        }
    }
}

