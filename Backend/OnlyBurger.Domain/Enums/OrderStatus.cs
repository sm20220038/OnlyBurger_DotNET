namespace OnlyBurger.Domain.Enums;

/// <summary>
/// Lifecycle state of an order. Orders start as <see cref="Pending"/> and can only be
/// modified or deleted by the customer while in that state.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
