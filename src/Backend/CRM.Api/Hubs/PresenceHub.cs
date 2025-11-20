using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Services;
using CRM.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using CRM.Application.Common.Persistence;

namespace CRM.Api.Hubs;

[Authorize]
public class PresenceHub : Hub
{
    private readonly IPresenceService _presenceService;
    private readonly IAppDbContext _db;

    public PresenceHub(IPresenceService presenceService, IAppDbContext db)
    {
        _presenceService = presenceService;
        _db = db;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await _presenceService.UpdatePresenceAsync(userId.Value, PresenceStatus.Online);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
            
            // Notify others that this user is now online
            await Clients.Others.SendAsync("UserPresenceChanged", userId.Value, "Online");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await _presenceService.UpdatePresenceAsync(userId.Value, PresenceStatus.Offline);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
            
            // Notify others that this user is now offline
            await Clients.Others.SendAsync("UserPresenceChanged", userId.Value, "Offline");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task UpdatePresence(string status)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        if (Enum.TryParse<PresenceStatus>(status, true, out var presenceStatus))
        {
            await _presenceService.UpdatePresenceAsync(userId.Value, presenceStatus);
            
            // Notify others of the presence change
            await Clients.Others.SendAsync("UserPresenceChanged", userId.Value, status);
        }
    }

    public async Task SubscribeToUsers(string[] userIds)
    {
        var currentUserId = GetUserId();
        if (!currentUserId.HasValue) return;

        // Add connection to groups for each user to monitor
        foreach (var userIdStr in userIds)
        {
            if (Guid.TryParse(userIdStr, out var userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
        }
    }

    public async Task UnsubscribeFromUsers(string[] userIds)
    {
        foreach (var userIdStr in userIds)
        {
            if (Guid.TryParse(userIdStr, out var userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
        }
    }

    private Guid? GetUserId()
    {
        var sub = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                  ?? Context.User?.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var userId) ? userId : null;
    }
}

