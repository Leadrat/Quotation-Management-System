using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Domain.UserManagement;
using CRM.Domain.UserManagement.Events;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Services;

public class ActivityService : IActivityService
{
    private readonly IAppDbContext _db;

    public ActivityService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task LogActivityAsync(Guid userId, string actionType, string? entityType, Guid? entityId, string ipAddress, string userAgent)
    {
        var activity = await CreateActivityAsync(userId, actionType, entityType, entityId, ipAddress, userAgent);
        await _db.SaveChangesAsync();
    }

    public async Task<UserActivity> CreateActivityAsync(Guid userId, string actionType, string? entityType, Guid? entityId, string ipAddress, string userAgent)
    {
        var activity = new UserActivity
        {
            ActivityId = Guid.NewGuid(),
            UserId = userId,
            ActionType = actionType,
            EntityType = entityType,
            EntityId = entityId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        _db.UserActivities.Add(activity);
        return activity;
    }
}

