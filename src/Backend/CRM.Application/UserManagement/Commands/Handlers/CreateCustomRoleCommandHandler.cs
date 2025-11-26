using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class CreateCustomRoleCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public CreateCustomRoleCommandHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<CustomRoleDto> Handle(CreateCustomRoleCommand cmd)
    {
        // Authorization: Only Admin can create custom roles
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin can create custom roles");
        }

        // Check if role name already exists
        var existingRole = await _db.Roles
            .FirstOrDefaultAsync(r => r.RoleName == cmd.Request.RoleName.Trim());
        if (existingRole != null)
        {
            throw new InvalidOperationException("Role with this name already exists");
        }

        var now = DateTime.UtcNow;
        var role = new Role
        {
            RoleId = Guid.NewGuid(),
            RoleName = cmd.Request.RoleName.Trim(),
            Description = cmd.Request.Description?.Trim(),
            IsBuiltIn = false,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Set permissions
        if (cmd.Request.Permissions != null && cmd.Request.Permissions.Any())
        {
            role.SetPermissions(cmd.Request.Permissions);
        }
        else
        {
            role.SetPermissions(new List<string>());
        }

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var roleWithNav = await _db.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.RoleId == role.RoleId);

        var dto = _mapper.Map<CustomRoleDto>(roleWithNav);
        dto.UserCount = roleWithNav?.UserRoles.Count ?? 0;

        return dto;
    }
}

