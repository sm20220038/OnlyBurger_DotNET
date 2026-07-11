using OnlyBurger.Domain.Enums;

namespace OnlyBurger.Domain.Entities;

/// <summary>
/// A system user. Can be a customer (<see cref="UserRole.User"/>) or restaurant
/// staff (<see cref="UserRole.Admin"/>).
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Contact number the restaurant calls when delivering the customer's order.</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>
    /// All of the user's carts: exactly one <see cref="Domain.Enums.CartStatus.Active"/> cart
    /// at any time, plus their checked-out carts kept as order history.
    /// </summary>
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
}
