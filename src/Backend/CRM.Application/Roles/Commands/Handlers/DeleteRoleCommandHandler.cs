using System;
using System.Threading.Tasks;
using CRM.Application.Roles.Commands;
using CRM.Application.Common.Persistence;
using CRM.Shared.Constants;
using CRM.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Roles.Commands.Handlers;

public class DeleteRoleCommandHandler
{
    private readonly IAppDbContext _db;

    public DeleteRoleCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteRoleCommand cmd)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == cmd.RoleId);
        if (role == null) throw new RoleNotFoundException($"Role '{cmd.RoleId}' not found");

        var isBuiltIn = role.RoleId == RoleConstants.AdminRoleId
            || role.RoleId == RoleConstants.ManagerRoleId
            || role.RoleId == RoleConstants.SalesRepRoleId
            || role.RoleId == RoleConstants.ClientRoleId;
        if (isBuiltIn) throw new CannotModifyBuiltInRoleException();

        var inUse = await _db.Users.AnyAsync(u => u.RoleId == role.RoleId && u.IsActive && u.DeletedAt == null);
        if (inUse) throw new CannotDeleteRoleInUseException();

        role.IsActive = false; // soft delete
        role.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
