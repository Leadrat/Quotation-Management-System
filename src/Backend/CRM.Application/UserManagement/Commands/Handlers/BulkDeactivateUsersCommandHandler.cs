using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Commands;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.UserManagement.Exceptions;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CRM.Application.UserManagement.Commands.Handlers;

public class BulkDeactivateUsersCommandHandler
{
    private readonly IAppDbContext _db;

    public BulkDeactivateUsersCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<BulkOperationResultDto> Handle(BulkDeactivateUsersCommand cmd)
    {
        // Authorization: Only Admin can bulk deactivate users
        var isAuthorized = string.Equals(cmd.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedTeamOperationException("Only Admin can bulk deactivate users");
        }

        var results = new List<BulkOperationItemResultDto>();
        var now = DateTime.UtcNow;

        var users = await _db.Users
            .Where(u => cmd.UserIds.Contains(u.UserId))
            .ToListAsync();

        foreach (var userId in cmd.UserIds)
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

                // Don't allow deactivating yourself
                if (user.UserId == cmd.DeactivatedByUserId)
                {
                    result.ErrorMessage = "Cannot deactivate yourself";
                    results.Add(result);
                    continue;
                }

                user.IsActive = false;
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
            TotalCount = cmd.UserIds.Count,
            SuccessCount = results.Count(r => r.Success),
            FailureCount = results.Count(r => !r.Success),
            Results = results
        };
    }
}

