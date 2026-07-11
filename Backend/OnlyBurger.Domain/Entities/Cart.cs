using OnlyBurger.Domain.Enums;

namespace OnlyBurger.Domain.Entities;

/// <summary>
/// A user's shopping cart. It is the staging area used to build an order before checkout,
/// and it is persisted in the database — so it survives a browser refresh, a new device,
/// or the server restarting (e.g. after a power outage).
///
/// A cart has a lifecycle: while <see cref="CartStatus.Active"/> the user edits its lines;
/// at checkout it becomes <see cref="CartStatus.CheckedOut"/> and is linked to the
/// <see cref="Entities.Order"/> it produced (see <see cref="Order"/>). A user therefore has
/// exactly one active cart plus any number of checked-out carts kept as history.
/// </summary>
public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public CartStatus Status { get; set; } = CartStatus.Active;

    /// <summary>When the cart was created (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the cart was turned into an order (UTC); null while it is still active.</summary>
    public DateTime? CheckedOutAt { get; set; }

    // Navigation
    public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    /// <summary>
    /// The order this cart produced at checkout, or null while the cart is still active.
    /// One cart maps to at most one order (1:1).
    /// </summary>
    public Order? Order { get; set; }
}
