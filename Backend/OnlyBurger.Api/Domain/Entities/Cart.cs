namespace OnlyBurger.Api.Domain.Entities;

/// <summary>
/// A user's shopping cart. It is the staging area used to build an order before checkout.
/// Each user owns exactly one cart, which in turn holds the cart lines.
/// </summary>
public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // Navigation
    public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
