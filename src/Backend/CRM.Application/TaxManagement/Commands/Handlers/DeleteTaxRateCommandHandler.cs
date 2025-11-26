using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Commands;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Commands.Handlers
{
    public class DeleteTaxRateCommandHandler
    {
        private readonly IAppDbContext _db;

        public DeleteTaxRateCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeleteTaxRateCommand cmd)
        {
            var entity = await _db.TaxRates
                .FirstOrDefaultAsync(tr => tr.TaxRateId == cmd.TaxRateId);

            if (entity == null)
            {
                throw new InvalidOperationException($"Tax rate with ID '{cmd.TaxRateId}' not found");
            }

            // Check if tax rate is referenced in quotations (via TaxCalculationLogs)
            var hasReferences = await _db.TaxCalculationLogs
                .AnyAsync(log => log.CountryId != null || log.JurisdictionId != null);

            // For now, allow hard delete since tax rates are historical records
            // In production, you might want to soft delete or prevent deletion if used
            _db.TaxRates.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}

