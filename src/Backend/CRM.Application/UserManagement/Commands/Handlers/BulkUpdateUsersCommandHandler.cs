using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class BulkUpdateUsersCommandHandler
{
    private readonly IAppDbContext _db;

    public BulkUpdateUsersCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<BulkOperationResultDto> Handle(BulkUpdateUsersCommand cmd)
    {
        // Authorization: Only Admin can bulk update users
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin can bulk update users");
        }

        var results = new List<BulkOperationItemResultDto>();
        var now = DateTime.UtcNow;

        var users = await _db.Users
            .Where(u => cmd.Request.UserIds.Contains(u.UserId))
            .ToListAsync();

        foreach (var userId in cmd.Request.UserIds)
        {
            var result = new BulkOperationItemResultDto
            {
                UserId = userId,
                Success = false
            };

            try
            {
                var user = users.FirstOrDefault(u => u.UserId == userId);
                if (user == null || user.DeletedAt != null)
                {
                    result.ErrorMessage = "User not found";
                    results.Add(result);
                    continue;
                }

                // Update IsActive if provided
                if (cmd.Request.IsActive.HasValue)
                {
                    user.IsActive = cmd.Request.IsActive.Value;
                }

                // Update RoleId if provided
                if (cmd.Request.RoleId.HasValue)
                {
                    var roleExists = await _db.Roles.AnyAsync(r => r.RoleId == cmd.Request.RoleId.Value && r.IsActive);
                    if (!roleExists)
                    {
                        result.ErrorMessage = "Invalid role ID";
                        results.Add(result);
                        continue;
                    }
                    user.RoleId = cmd.Request.RoleId.Value;
                }

                // TODO: Update TeamId if provided

                user.UpdatedAt = now;
                result.UserEmail = user.Email;
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
            TotalCount = cmd.Request.UserIds.Count,
            SuccessCount = results.Count(r => r.Success),
            FailureCount = results.Count(r => !r.Success),
            Results = results
        };
    }
}

