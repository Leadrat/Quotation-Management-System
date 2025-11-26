using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class UpdateRolePermissionsCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public UpdateRolePermissionsCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<CustomRoleDto> Handle(UpdateRolePermissionsCommand cmd)
    {
        // Authorization: Only Admin can update role permissions
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin can update role permissions");
        }

        var role = await _db.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.RoleId == cmd.RoleId);

        if (role == null)
        {
            throw new InvalidOperationException("Role not found");
        }

        // Cannot modify built-in roles
        if (role.IsBuiltIn)
        {
            throw new InvalidOperationException("Cannot modify built-in roles");
        }

        // Update permissions
        if (cmd.Request.Permissions != null)
        {
            role.SetPermissions(cmd.Request.Permissions);
        }
        else
        {
            role.SetPermissions(new List<string>());
        }

        role.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var updatedRole = await _db.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.RoleId == role.RoleId);

        var dto = _mapper.Map<CustomRoleDto>(updatedRole);
        dto.UserCount = updatedRole?.UserRoles.Count ?? 0;

        return dto;
    }
}

