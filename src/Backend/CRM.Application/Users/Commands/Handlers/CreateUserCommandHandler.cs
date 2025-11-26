using System;
using System.Threading.Tasks;
using CRM.Application.Users.Commands.Results;
using CRM.Domain.Entities;
using CRM.Domain.Events;
using CRM.Application.Common.Interfaces;
using CRM.Application.Common.Persistence;
using CRM.Shared.Constants;
using CRM.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using CRM.Shared.Exceptions;
using System.Text.RegularExpressions;

namespace CRM.Application.Users.Commands.Handlers;

public class CreateUserCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public CreateUserCommandHandler(IAppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<UserCreatedResult> Handle(CreateUserCommand cmd)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();
        var currentTenantId = _tenantContext.CurrentTenantId;
        var exists = await _db.Users.AnyAsync(u => u.Email == email && (u.TenantId == currentTenantId || u.TenantId == null));
        if (exists)
        {
            throw new InvalidOperationException("Email already exists in current tenant");
        }

        if (cmd.RoleId == RoleIds.Client)
        {
            throw new DomainValidationException("Admin cannot create Client users using this endpoint");
        }

        // SuperAdmin role is global, other roles are tenant-specific
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == cmd.RoleId && 
            (r.RoleName == "SuperAdmin" || cmd.RoleId != RoleIds.SuperAdmin));
        if (role == null)
        {
            throw new DomainValidationException("Invalid role");
        }

        Guid? managerId = cmd.ReportingManagerId;
        // Validate reporting manager if provided (optional for all roles including SalesRep)
        if (managerId != null)
        {
            var manager = await _db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == managerId.Value && (u.TenantId == currentTenantId || u.TenantId == null));
            if (manager == null || !manager.IsActive || manager.RoleId != RoleIds.Manager)
            {
                throw new DomainValidationException("Invalid reporting manager");
            }
        }

        var now = DateTime.UtcNow;
        var firstName = Regex.Replace(cmd.FirstName ?? string.Empty, "<.*?>", string.Empty).Trim();
        var lastName = Regex.Replace(cmd.LastName ?? string.Empty, "<.*?>", string.Empty).Trim();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            TenantId = currentTenantId,
            Email = email,
            PasswordHash = PasswordHelper.HashPassword(cmd.Password),
            FirstName = firstName,
            LastName = lastName,
            Mobile = string.IsNullOrWhiteSpace(cmd.Mobile) ? null : cmd.Mobile,
            PhoneCode = string.IsNullOrWhiteSpace(cmd.PhoneCode) ? null : cmd.PhoneCode,
            IsActive = true,
            RoleId = cmd.RoleId,
            ReportingManagerId = managerId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var ev = new AdminUserCreated
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleId = user.RoleId,
            RoleName = role.RoleName,
            ReportingManagerId = user.ReportingManagerId,
            ReportingManagerName = null,
            CreatedAt = now,
            CreatedBy = cmd.CreatedByUserId.ToString(),
            TemporaryPasswordExpiry = now.AddDays(7)
        };
        _ = ev;

        return new UserCreatedResult
        {
            Success = true,
            Message = "User created",
            UserId = user.UserId,
            Email = user.Email,
            RoleId = user.RoleId,
            EmailSent = false,
            TemporaryPasswordExpiry = ev.TemporaryPasswordExpiry
        };
    }
}
