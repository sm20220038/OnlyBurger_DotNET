namespace OnlyBurger.Domain.Entities;

/// <summary>
/// A menu item that can be ordered (e.g. Classic Burger, Fries, Coca-Cola).
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
