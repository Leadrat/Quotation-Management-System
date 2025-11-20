using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Services;

public class PresenceService : IPresenceService
{
    private readonly IAppDbContext _db;

    public PresenceService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task UpdatePresenceAsync(Guid userId, PresenceStatus status)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user != null && user.IsActive && user.DeletedAt == null)
        {
            user.UpdatePresence(status);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<Guid, PresenceStatus>> GetPresenceStatusesAsync(IEnumerable<Guid> userIds)
    {
        var userIdList = userIds.ToList();
        if (!userIdList.Any())
        {
            return new Dictionary<Guid, PresenceStatus>();
        }

        var users = await _db.Users
            .AsNoTracking()
            .Where(u => userIdList.Contains(u.UserId) && u.IsActive && u.DeletedAt == null)
            .Select(u => new { u.UserId, u.PresenceStatus })
            .ToListAsync();

        return users.ToDictionary(u => u.UserId, u => u.PresenceStatus);
    }

    public Task NotifyPresenceUpdateAsync(Guid userId, PresenceStatus status)
    {
        // This will be called by SignalR hub to notify clients
        // Implementation is handled by the hub itself
        return Task.CompletedTask;
    }
}

