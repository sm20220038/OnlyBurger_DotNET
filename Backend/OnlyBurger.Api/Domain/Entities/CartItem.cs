namespace OnlyBurger.Api.Domain.Entities;

/// <summary>
/// An entry in a shopping cart. One row per (cart, product) pair; the quantity tracks how
/// many of that product are staged for the order.
/// </summary>
public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    // Navigation
    public Cart? Cart { get; set; }
    public Product? Product { get; set; }
}
