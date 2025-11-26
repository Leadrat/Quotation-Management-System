using System;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Commands;
using CRM.Application.Common.Persistence;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class UpdatePresenceCommandHandler
{
    private readonly IAppDbContext _db;

    public UpdatePresenceCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(UpdatePresenceCommand cmd)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserId == cmd.UserId);

        if (user == null || !user.IsActive || user.DeletedAt != null)
        {
            throw new InvalidOperationException("User not found or inactive");
        }

        user.UpdatePresence(cmd.Status);
        await _db.SaveChangesAsync();
    }
}

