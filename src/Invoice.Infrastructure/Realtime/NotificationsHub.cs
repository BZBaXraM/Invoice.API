using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Invoice.Infrastructure.Realtime;

/// <summary>
/// Live-update hub. Clients connect once authenticated and are placed into a group named
/// after their user id, so server-side changes can be pushed only to that user's sessions.
/// </summary>
[Authorize]
public class NotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(userId));
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(userId));
        }

        await base.OnDisconnectedAsync(exception);
    }

    public static string GroupName(string userId) => $"user:{userId}";
}
