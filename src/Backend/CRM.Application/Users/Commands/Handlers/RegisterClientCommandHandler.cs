using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Users.Commands.Results;
using CRM.Domain.Entities;
using CRM.Domain.Events;
using CRM.Application.Common.Persistence;
using CRM.Shared.Constants;
using CRM.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CRM.Application.Users.Commands.Handlers;

public class RegisterClientCommandHandler
{
    private readonly IAppDbContext _db;

    public RegisterClientCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<RegisterResult> Handle(RegisterClientCommand cmd)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();
        if (!Regex.IsMatch(cmd.Password, ValidationConstants.PasswordRegex))
        {
            throw new ArgumentException("Weak password");
        }
        // Uniqueness check (citext unique index)
        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists)
        {
            throw new InvalidOperationException("Email already exists");
        }

        var now = DateTime.UtcNow;
        var firstName = Regex.Replace(cmd.FirstName ?? string.Empty, "<.*?>", string.Empty).Trim();
        var lastName = Regex.Replace(cmd.LastName ?? string.Empty, "<.*?>", string.Empty).Trim();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = email,
            PasswordHash = PasswordHelper.HashPassword(cmd.Password),
            FirstName = firstName,
            LastName = lastName,
            Mobile = string.IsNullOrWhiteSpace(cmd.Mobile) ? null : cmd.Mobile,
            PhoneCode = string.IsNullOrWhiteSpace(cmd.PhoneCode) ? null : cmd.PhoneCode,
            IsActive = true,
            RoleId = RoleIds.Client,
            ReportingManagerId = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Emit domain event (no bus wired; placeholder object creation)
        var ev = new UserCreated
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleId = user.RoleId,
            RoleName = "Client",
            CreatedAt = now,
            CreatedBy = null
        };
        _ = ev;

        return new RegisterResult
        {
            Success = true,
            UserId = user.UserId,
            Email = user.Email,
            Message = "Account created successfully. Please login.",
            RedirectUrl = "/login"
        };
    }
}
