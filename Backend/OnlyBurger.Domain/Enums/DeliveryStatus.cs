namespace OnlyBurger.Domain.Enums;

/// <summary>
/// Tracks how far along an approved order is on its way to the customer. Advanced by
/// restaurant staff and pushed to the customer in real time over SignalR.
/// </summary>
public enum DeliveryStatus
{
    /// <summary>Not started yet (the order is still pending/just approved).</summary>
    Pending = 0,

    /// <summary>The kitchen is preparing the food.</summary>
    Preparing = 1,

    /// <summary>A courier has picked the order up and is on the way.</summary>
    OutForDelivery = 2,

    /// <summary>The order has reached the customer.</summary>
    Delivered = 3
}
