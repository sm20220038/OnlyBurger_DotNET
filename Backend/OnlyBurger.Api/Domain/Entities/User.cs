using OnlyBurger.Api.Domain.Enums;

namespace OnlyBurger.Api.Domain.Entities;

/// <summary>
/// A system user. Can be a customer (<see cref="UserRole.User"/>) or restaurant
/// staff (<see cref="UserRole.Admin"/>).
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public Cart? Cart { get; set; }
}
