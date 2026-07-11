using Microsoft.AspNetCore.SignalR;
using OnlyBurger.Infrastructure.Features.Orders;

namespace OnlyBurger.Infrastructure.Realtime;

/// <summary>
/// Pushes order changes to the owning customer in real time. Command handlers depend on this
/// abstraction instead of talking to SignalR directly, which keeps them easy to read and test.
/// </summary>
public interface IOrderNotifier
{
    /// <summary>Sends the latest state of an order to its owner over SignalR.</summary>
    Task OrderUpdatedAsync(int ownerUserId, OrderDto order, CancellationToken cancellationToken = default);

    /// <summary>Notifies every connected admin that the set of orders changed (e.g. a new order was placed).</summary>
    Task AdminOrdersChangedAsync(OrderDto order, CancellationToken cancellationToken = default);
}

/// <summary>SignalR-backed implementation of <see cref="IOrderNotifier"/>.</summary>
public class OrderNotifier : IOrderNotifier
{
    private readonly IHubContext<OrderTrackingHub> _hub;

    public OrderNotifier(IHubContext<OrderTrackingHub> hub) => _hub = hub;

    public Task OrderUpdatedAsync(int ownerUserId, OrderDto order, CancellationToken cancellationToken = default)
        => _hub.Clients
            .User(ownerUserId.ToString())
            .SendAsync(OrderTrackingHub.OrderUpdatedEvent, order, cancellationToken);

    public Task AdminOrdersChangedAsync(OrderDto order, CancellationToken cancellationToken = default)
        => _hub.Clients
            .Group(OrderTrackingHub.AdminGroup)
            .SendAsync(OrderTrackingHub.AdminOrdersChangedEvent, order, cancellationToken);
}
