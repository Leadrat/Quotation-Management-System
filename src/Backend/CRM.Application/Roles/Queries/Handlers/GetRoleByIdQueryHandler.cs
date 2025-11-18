using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Roles.Queries;
using CRM.Application.Common.Persistence;
using CRM.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Roles.Queries.Handlers;

public class GetRoleByIdQueryHandler
{
    private readonly IAppDbContext _db;

    public GetRoleByIdQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<RoleDto> Handle(GetRoleByIdQuery query)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == query.RoleId);
        if (role == null)
        {
            throw new RoleNotFoundException($"Role '{query.RoleId}' not found");
        }
        var count = await _db.Users.CountAsync(u => u.RoleId == role.RoleId && u.DeletedAt == null && u.IsActive);
        return new RoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Description = role.Description,
            IsActive = role.IsActive,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            UserCount = count
        };
    }
}
