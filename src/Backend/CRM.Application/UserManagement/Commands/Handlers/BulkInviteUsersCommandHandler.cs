using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Security;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class BulkInviteUsersCommandHandler
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public BulkInviteUsersCommandHandler(IAppDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<BulkOperationResultDto> Handle(BulkInviteUsersCommand cmd)
    {
        // Authorization: Only Admin can bulk invite users
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin can bulk invite users");
        }

        var results = new List<BulkOperationItemResultDto>();
        var now = DateTime.UtcNow;

        foreach (var userItem in cmd.Request.Users)
        {
            var result = new BulkOperationItemResultDto
            {
                UserEmail = userItem.Email,
                Success = false
            };

            try
            {
                // Check if user already exists
                var existingUser = await _db.Users
                    .FirstOrDefaultAsync(u => u.Email == userItem.Email);

                if (existingUser != null)
                {
                    result.ErrorMessage = "User with this email already exists";
                    results.Add(result);
                    continue;
                }

                // Validate role if provided
                if (cmd.Request.RoleId.HasValue)
                {
                    var roleExists = await _db.Roles.AnyAsync(r => r.RoleId == cmd.Request.RoleId.Value && r.IsActive);
                    if (!roleExists)
                    {
                        result.ErrorMessage = "Invalid role ID";
                        results.Add(result);
                        continue;
                    }
                }

                // Create new user
                var tempPassword = Guid.NewGuid().ToString("N")[..12]; // Generate temporary password
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = userItem.Email.Trim().ToLowerInvariant(),
                    PasswordHash = _passwordHasher.Hash(tempPassword),
                    FirstName = userItem.FirstName.Trim(),
                    LastName = userItem.LastName.Trim(),
                    Mobile = userItem.Mobile?.Trim(),
                    IsActive = true,
                    RoleId = cmd.Request.RoleId ?? Guid.Empty, // Will need to be set properly
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _db.Users.Add(user);

                // TODO: Add to team if TeamId provided
                // TODO: Send email invite if SendEmailInvites is true

                result.UserId = user.UserId;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            results.Add(result);
        }

        await _db.SaveChangesAsync();

        return new BulkOperationResultDto
        {
            TotalCount = cmd.Request.Users.Count,
            SuccessCount = results.Count(r => r.Success),
            FailureCount = results.Count(r => !r.Success),
            Results = results
        };
    }
}

