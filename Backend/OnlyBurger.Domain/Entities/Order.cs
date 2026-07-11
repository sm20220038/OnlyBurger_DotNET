using OnlyBurger.Domain.Enums;

namespace OnlyBurger.Domain.Entities;

/// <summary>
/// A customer order created from the contents of the shopping cart.
/// </summary>
public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }

    /// <summary>The cart this order was created from (1:1). Null only for legacy orders.</summary>
    public int? CartId { get; set; }

    public string DeliveryLocation { get; set; } = string.Empty;
    public DateTime OrderDateTime { get; set; }
    public decimal TotalPrice { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    /// <summary>How far along delivery is. Advanced by staff and streamed to the customer live.</summary>
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Pending;

    // Navigation
    public User? User { get; set; }
    public Cart? Cart { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Recomputes <see cref="TotalPrice"/> from the line items. Total order price is
    /// always derived from the items, never set directly by the client.
    /// </summary>
    public void RecalculateTotal()
    {
        TotalPrice = Items.Sum(i => i.UnitPrice * i.Quantity);
    }
}
