namespace OnlyBurger.Domain.Entities;

/// <summary>
/// A single line within an order. <see cref="UnitPrice"/> is captured at order time so
/// later menu price changes do not alter historical orders.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    // Navigation
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
