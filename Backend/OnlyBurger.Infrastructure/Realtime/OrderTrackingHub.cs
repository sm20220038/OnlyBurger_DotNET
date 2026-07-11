using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OnlyBurger.Infrastructure.Realtime;

/// <summary>
/// SignalR hub that streams live order + delivery updates to the customer who owns the order.
///
/// Clients don't call any methods here — they just connect (authenticated with their JWT) and
/// listen for the <c>OrderUpdated</c> event. The server pushes to a specific user with
/// <c>Clients.User(userId)</c>; SignalR's default user-id provider maps that to the
/// <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/> claim, which this API sets to
/// the user's id when it issues the token.
/// </summary>
[Authorize]
public class OrderTrackingHub : Hub
{
    /// <summary>The client-side event name customers subscribe to for live order changes.</summary>
    public const string OrderUpdatedEvent = "OrderUpdated";

    /// <summary>The client-side event name the admin orders page subscribes to.</summary>
    public const string AdminOrdersChangedEvent = "AdminOrdersChanged";

    /// <summary>SignalR group every connected admin joins, used to broadcast order changes to staff.</summary>
    public const string AdminGroup = "admins";

    /// <summary>Put admin connections into the <see cref="AdminGroup"/> so they can be notified of new orders.</summary>
    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole("Admin") == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
        }

        await base.OnConnectedAsync();
    }
}
