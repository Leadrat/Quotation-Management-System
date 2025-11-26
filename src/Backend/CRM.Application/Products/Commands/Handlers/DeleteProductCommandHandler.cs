using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Products.Commands.Handlers
{
    public class DeleteProductCommandHandler
    {
        private readonly IAppDbContext _db;

        public DeleteProductCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeleteProductCommand cmd)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.ProductId == cmd.ProductId);

            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {cmd.ProductId} not found.");
            }

            // Check if product is used in any quotations
            var isUsed = await _db.QuotationLineItems
                .AnyAsync(li => li.ProductId == cmd.ProductId);

            if (isUsed)
            {
                // Soft delete by setting IsActive to false
                product.IsActive = false;
                product.UpdatedByUserId = cmd.DeletedByUserId;
                product.UpdatedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                // Hard delete if not used
                _db.Products.Remove(product);
            }

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new InvalidOperationException($"Database error while deleting product: {innerException}", dbEx);
            }
        }
    }
}

