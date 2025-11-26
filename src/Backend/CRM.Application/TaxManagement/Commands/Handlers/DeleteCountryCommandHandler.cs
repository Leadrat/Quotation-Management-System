using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Commands;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Commands.Handlers
{
    public class DeleteCountryCommandHandler
    {
        private readonly IAppDbContext _db;

        public DeleteCountryCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeleteCountryCommand cmd)
        {
            var entity = await _db.Countries
                .FirstOrDefaultAsync(c => c.CountryId == cmd.CountryId && c.DeletedAt == null);
            
            if (entity == null)
            {
                throw new InvalidOperationException($"Country with ID '{cmd.CountryId}' not found");
            }

            // Check if country is in use by clients
            var clientsUsingCountry = await _db.Clients
                .Where(c => c.CountryId == cmd.CountryId && c.DeletedAt == null)
                .ToListAsync();
            
            if (clientsUsingCountry.Any())
            {
                throw new InvalidOperationException($"Cannot delete country '{entity.CountryName}' because it is in use by {clientsUsingCountry.Count} client(s)");
            }

            // Soft delete
            var now = DateTimeOffset.UtcNow;
            entity.DeletedAt = now;
            entity.UpdatedAt = now;
            entity.IsActive = false;
            entity.IsDefault = false;

            // Soft delete all jurisdictions for this country
            var jurisdictions = await _db.Jurisdictions
                .Where(j => j.CountryId == cmd.CountryId && j.DeletedAt == null)
                .ToListAsync();
            
            foreach (var jurisdiction in jurisdictions)
            {
                jurisdiction.DeletedAt = now;
                jurisdiction.UpdatedAt = now;
                jurisdiction.IsActive = false;
            }

            await _db.SaveChangesAsync();
        }
    }
}

