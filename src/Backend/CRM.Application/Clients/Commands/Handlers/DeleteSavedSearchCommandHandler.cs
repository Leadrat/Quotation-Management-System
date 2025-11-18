using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Commands.Handlers
{
    public class DeleteSavedSearchCommandHandler
    {
        private readonly IAppDbContext _db;
        public DeleteSavedSearchCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeleteSavedSearchCommand cmd)
        {
            var entity = await _db.SavedSearches.FirstOrDefaultAsync(s => s.SavedSearchId == cmd.SavedSearchId);
            if (entity == null)
            {
                return; // nothing to delete
            }

            if (!cmd.IsAdmin && entity.UserId != cmd.UserId)
            {
                throw new UnauthorizedAccessException("Cannot delete a saved search owned by another user.");
            }

            entity.IsActive = false;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
