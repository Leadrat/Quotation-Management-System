using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Commands;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Commands.Handlers
{
    public class DeleteJurisdictionCommandHandler
    {
        private readonly IAppDbContext _db;

        public DeleteJurisdictionCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeleteJurisdictionCommand cmd)
        {
            var entity = await _db.Jurisdictions
                .FirstOrDefaultAsync(j => j.JurisdictionId == cmd.JurisdictionId && j.DeletedAt == null);

            if (entity == null)
            {
                throw new InvalidOperationException($"Jurisdiction with ID '{cmd.JurisdictionId}' not found");
            }

            // Check if jurisdiction is in use (has tax rates or clients)
            var hasTaxRates = await _db.TaxRates.AnyAsync(tr => tr.JurisdictionId == cmd.JurisdictionId);
            var hasClients = await _db.Clients.AnyAsync(c => c.JurisdictionId == cmd.JurisdictionId);

            if (hasTaxRates || hasClients)
            {
                // Soft delete
                entity.DeletedAt = DateTimeOffset.UtcNow;
                entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                // Hard delete if no dependencies
                _db.Jurisdictions.Remove(entity);
            }

            await _db.SaveChangesAsync();
        }
    }
}

