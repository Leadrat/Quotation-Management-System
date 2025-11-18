using System;
using System.Threading.Tasks;
using CRM.Application.Roles.Commands;
using CRM.Application.Roles.Queries;
using CRM.Application.Common.Persistence;
using CRM.Shared.Constants;
using CRM.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Roles.Commands.Handlers;

public class UpdateRoleCommandHandler
{
    private readonly IAppDbContext _db;

    public UpdateRoleCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<RoleDto> Handle(UpdateRoleCommand cmd)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == cmd.RoleId);
        if (role == null) throw new RoleNotFoundException($"Role '{cmd.RoleId}' not found");

        var isBuiltIn = role.RoleId == RoleConstants.AdminRoleId
            || role.RoleId == RoleConstants.ManagerRoleId
            || role.RoleId == RoleConstants.SalesRepRoleId
            || role.RoleId == RoleConstants.ClientRoleId;

        if (isBuiltIn)
        {
            // built-ins are immutable: cannot rename or deactivate
            if (!string.IsNullOrWhiteSpace(cmd.RoleName) || (cmd.IsActive.HasValue && cmd.IsActive.Value == false))
            {
                throw new CannotModifyBuiltInRoleException();
            }
        }

        if (!string.IsNullOrWhiteSpace(cmd.RoleName))
        {
            var name = cmd.RoleName!.Trim();
            if (name.Length < 3 || name.Length > 100) throw new InvalidRoleException("RoleName length invalid");
            var normalized = name.ToLowerInvariant();
            var exists = await _db.Roles.AnyAsync(r => r.RoleId != role.RoleId && r.RoleName.ToLower() == normalized);
            if (exists) throw new DuplicateRoleNameException(name);
            role.RoleName = name;
        }

        if (cmd.Description != null)
        {
            role.Description = string.IsNullOrWhiteSpace(cmd.Description) ? null : cmd.Description.Trim();
        }

        if (cmd.IsActive.HasValue)
        {
            if (!cmd.IsActive.Value)
            {
                var inUse = await _db.Users.AnyAsync(u => u.RoleId == role.RoleId && u.IsActive && u.DeletedAt == null);
                if (inUse) throw new CannotDeleteRoleInUseException("Cannot deactivate role with active users");
            }
            role.IsActive = cmd.IsActive.Value;
        }

        role.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var userCount = await _db.Users.CountAsync(u => u.RoleId == role.RoleId && u.IsActive && u.DeletedAt == null);
        return new RoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Description = role.Description,
            IsActive = role.IsActive,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            UserCount = userCount
        };
    }
}
