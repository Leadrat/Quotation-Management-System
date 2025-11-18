using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Roles.Commands;
using CRM.Application.Roles.Queries;
using CRM.Application.Common.Persistence;
using CRM.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Roles.Commands.Handlers;

public class CreateRoleCommandHandler
{
    private readonly IAppDbContext _db;

    public CreateRoleCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand cmd)
    {
        var name = (cmd.RoleName ?? string.Empty).Trim();
        if (name.Length < 3 || name.Length > 100) throw new InvalidRoleException("RoleName length invalid");
        var normalized = name.ToLowerInvariant();
        var exists = await _db.Roles.AnyAsync(r => r.RoleName.ToLower() == normalized);
        if (exists) throw new DuplicateRoleNameException(name);

        var now = DateTime.UtcNow;
        var role = new CRM.Domain.Entities.Role
        {
            RoleId = Guid.NewGuid(),
            RoleName = name,
            Description = string.IsNullOrWhiteSpace(cmd.Description) ? null : cmd.Description!.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        var userCount = 0;
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
