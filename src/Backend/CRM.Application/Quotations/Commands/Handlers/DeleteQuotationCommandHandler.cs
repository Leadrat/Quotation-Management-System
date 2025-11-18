using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Quotations.Exceptions;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Quotations.Commands.Handlers
{
    public class DeleteQuotationCommandHandler
    {
        private readonly IAppDbContext _db;

        public DeleteQuotationCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeleteQuotationCommand command)
        {
            var quotation = await _db.Quotations
                .FirstOrDefaultAsync(q => q.QuotationId == command.QuotationId);

            if (quotation == null)
            {
                throw new QuotationNotFoundException(command.QuotationId);
            }

            // Authorization: User owns quotation or is admin
            var isAdmin = string.Equals(command.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && quotation.CreatedByUserId != command.DeletedByUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this quotation.");
            }

            // Only draft or cancelled quotations can be deleted
            if (quotation.Status != QuotationStatus.Draft && quotation.Status != QuotationStatus.Cancelled)
            {
                throw new InvalidQuotationStatusException("Only draft or cancelled quotations can be deleted.");
            }

            // Soft delete: Set status to Cancelled
            quotation.Status = QuotationStatus.Cancelled;
            quotation.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}

