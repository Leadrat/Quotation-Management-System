using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CRM.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public async Task JoinUserGroup()
    {
        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.User?.FindFirstValue("sub");
        if (!string.IsNullOrEmpty(userIdClaim))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userIdClaim}");
        }
    }

    public async Task LeaveUserGroup()
    {
        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.User?.FindFirstValue("sub");
        if (!string.IsNullOrEmpty(userIdClaim))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userIdClaim}");
        }
    }

    public override async Task OnConnectedAsync()
    {
        await JoinUserGroup();
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await LeaveUserGroup();
        await base.OnDisconnectedAsync(exception);
    }
}