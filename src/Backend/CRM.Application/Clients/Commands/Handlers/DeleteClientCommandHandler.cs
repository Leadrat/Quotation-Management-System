using System;
using System.Threading.Tasks;
using CRM.Application.Common.Results;
using CRM.Application.Clients.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Commands.Handlers
{
    public class DeleteClientCommandHandler
    {
        private readonly IAppDbContext _db;
        public DeleteClientCommandHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<DeleteResult> Handle(DeleteClientCommand cmd)
        {
            var entity = await _db.Clients.FirstOrDefaultAsync(c => c.ClientId == cmd.ClientId && c.DeletedAt == null);
            if (entity == null) throw new ClientNotFoundException(cmd.ClientId);

            var isAdmin = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && entity.CreatedByUserId != cmd.DeletedByUserId)
            {
                throw new UnauthorizedAccessException("Cannot delete other user's client");
            }

            entity.DeletedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            return new DeleteResult { Success = true, Message = "Client deleted successfully", DeletedAt = entity.DeletedAt.Value };
        }
    }
}
